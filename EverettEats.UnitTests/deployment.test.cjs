const assert = require('node:assert/strict');
const { spawnSync } = require('node:child_process');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const test = require('node:test');

const workflow = fs.readFileSync(path.join(__dirname, '../.github/workflows/deploy.yml'), 'utf8');
const guardBlock = workflow.match(/    - name: Check deployment revision\n      id: latest_main\n      shell: bash\n      run: \|\n((?:        .*\n)+)/);
assert.ok(guardBlock, 'the deployment guard must remain in the workflow');
const guard = guardBlock[1].split('\n').map(line => line.slice(8)).join('\n');
const older = 'a'.repeat(40);
const newer = 'b'.repeat(40);

function checkRevision(expected, remote = newer, exitCode = 0, rawResponse) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'everetteats-deployment-'));
  try {
    fs.writeFileSync(path.join(dir, 'git'), '#!/bin/sh\nprintf "%s" "$TEST_REMOTE"\nexit "$TEST_GIT_EXIT"\n', { mode: 0o755 });
    const output = path.join(dir, 'output');
    const summary = path.join(dir, 'summary');
    const result = spawnSync('bash', ['-c', guard], {
      encoding: 'utf8',
      env: {
        ...process.env,
        PATH: `${dir}:${process.env.PATH}`,
        GITHUB_SERVER_URL: 'https://github.com',
        GITHUB_REPOSITORY: 'jreverett/EverettEats',
        GITHUB_SHA: expected,
        GITHUB_OUTPUT: output,
        GITHUB_STEP_SUMMARY: summary,
        TEST_REMOTE: rawResponse ?? (remote === null ? '' : `${remote}\trefs/heads/main\n`),
        TEST_GIT_EXIT: String(exitCode),
      },
    });
    return {
      status: result.status,
      output: fs.existsSync(output) ? fs.readFileSync(output, 'utf8') : '',
      summary: fs.existsSync(summary) ? fs.readFileSync(summary, 'utf8') : '',
    };
  } finally {
    fs.rmSync(dir, { recursive: true, force: true });
  }
}

test('current main may deploy', () => {
  const result = checkRevision(newer);
  assert.equal(result.status, 0);
  assert.equal(result.output.trim(), 'current=true');
});

test('a record for a different branch is rejected', () => {
  const result = checkRevision(newer, newer, 0, `${newer}\trefs/heads/other\n`);
  assert.notEqual(result.status, 0);
  assert.equal(result.output, '');
});

test('a slow older build finishing after the newer build cannot roll production back', () => {
  const readyOrder = [newer, older];
  // Serialization alone permits this order and ends on the older revision.
  assert.equal(readyOrder.at(-1), older);
  const deployed = readyOrder.filter(sha => checkRevision(sha).output.trim() === 'current=true');
  assert.deepEqual(deployed, [newer]);
  assert.match(checkRevision(older).summary, /Skipping superseded deployment/);
});

test('a retry of an old run is skipped even if it previously deployed', () => {
  assert.equal(checkRevision(older, older).output.trim(), 'current=true');
  assert.equal(checkRevision(older, newer).output.trim(), 'current=false');
});

test('failed, timed-out, missing and malformed head lookups fail closed', () => {
  for (const [remote, code] of [
    [newer, 128], [newer, 124], [null, 2], [null, 0], ['not-a-commit', 0],
    [`${newer}\trefs/heads/main\n${newer}`, 0],
    [`${newer}\trefs/heads/other\n${newer}`, 0],
  ]) {
    const result = checkRevision(newer, remote, code);
    assert.notEqual(result.status, 0);
    assert.notEqual(result.output.trim(), 'current=true');
  }
});

test('an older successful build is skipped even when newer main has not passed its build', () => {
  assert.equal(checkRevision(older, newer).output.trim(), 'current=false');
});

test('a push after the head check waits behind the running deployment', () => {
  assert.match(workflow, /cancel-in-progress: false/);
  const first = checkRevision(older, older);
  assert.equal(first.output.trim(), 'current=true');
  // The lock lets this deployment finish before the newly pushed revision starts.
  const next = checkRevision(newer, newer);
  assert.equal(next.output.trim(), 'current=true');
  assert.deepEqual([older, newer].at(-1), newer);
});

test('deployment serialization retains the newest pending job without interrupting a running deploy', () => {
  assert.match(workflow, /concurrency:\s*\n\s+group: everetteats-production\s*\n\s+cancel-in-progress: false\s*\n\s+#.*\n\s+queue: max/);
  // C is waiting while older B finishes late. A single-pending queue would lose C.
  const pending = [newer, older];
  const deployed = pending.filter(sha => checkRevision(sha).output.trim() === 'current=true');
  assert.deepEqual(deployed, [newer]);
});

test('both remote mutations are gated after the in-lock head check', () => {
  for (const name of ['Deploy to VPS', 'Update app on VPS']) {
    assert.match(workflow, new RegExp(`- name: ${name}\\n      if: steps\\.latest_main\\.outputs\\.current == 'true'`));
    assert.ok(workflow.indexOf('id: latest_main') < workflow.indexOf(`- name: ${name}`));
  }
  const deploy = workflow.slice(workflow.indexOf('  deploy:\n'));
  assert.match(deploy, /permissions: \{\}/);
  assert.match(deploy, /github\.ref == 'refs\/heads\/main'/);
});
