const assert = require('node:assert/strict');
const { webcrypto } = require('node:crypto');
const fs = require('node:fs');
const path = require('node:path');
const test = require('node:test');
const vm = require('node:vm');

const source = fs.readFileSync(path.join(__dirname, '../wwwroot/js/kitchen.js'), 'utf8');

function loadKitchen(fetch, crypto = webcrypto) {
  const context = {
    window: {}, crypto, fetch, Uint8Array, TextEncoder,
    atob: value => Buffer.from(value, 'base64').toString('binary'),
  };
  vm.runInNewContext(source, context);
  return context.window.kitchenOpen;
}

async function encryptFixture(text, phrase) {
  const salt = webcrypto.getRandomValues(new Uint8Array(16));
  const iv = webcrypto.getRandomValues(new Uint8Array(12));
  const material = await webcrypto.subtle.importKey(
    'raw', new TextEncoder().encode(phrase), 'PBKDF2', false, ['deriveKey']);
  const key = await webcrypto.subtle.deriveKey(
    { name: 'PBKDF2', salt, iterations: 150000, hash: 'SHA-256' },
    material, { name: 'AES-GCM', length: 256 }, false, ['encrypt']);
  const data = await webcrypto.subtle.encrypt(
    { name: 'AES-GCM', iv }, key, new TextEncoder().encode(text));
  return { data: Buffer.concat([salt, iv, Buffer.from(data)]).toString('base64') };
}

test('drafts larger than 32 KB return bytes for streaming, preserving UTF-8', async () => {
  const html = '<p>' + 'bread'.repeat(16000) + ' 🥖 café</p>';
  const wrapper = await encryptFixture(html, 'testphrase');
  const kitchenOpen = loadKitchen(async (url, options) => {
    assert.equal(url, '/fixture.json');
    assert.equal(options.cache, 'no-store');
    return { ok: true, json: async () => wrapper };
  });
  const bytes = await kitchenOpen('/fixture.json', ' Test-Phrase! ');
  assert.ok(bytes instanceof Uint8Array);
  assert.ok(bytes.length > 32 * 1024);
  assert.equal(new TextDecoder().decode(bytes), html);
});

test('wrong phrase rejects without returning draft content', async () => {
  const wrapper = await encryptFixture('<p>fixture only</p>', 'rightphrase');
  const kitchenOpen = loadKitchen(async () => ({ ok: true, json: async () => wrapper }));
  await assert.rejects(kitchenOpen('/fixture.json', 'wrongphrase'));
});

test('missing draft and insecure crypto context fail closed', async () => {
  await assert.rejects(loadKitchen(async () => ({ ok: false }))('/missing.json', ''), /not found/);
  await assert.rejects(loadKitchen(null, null)('/fixture.json', ''), /secure context/);
});

test('current encrypted draft fits the bounded reader without decrypting it', () => {
  const wrapper = JSON.parse(fs.readFileSync(
    path.join(__dirname, '../wwwroot/data/impossible-sourdough.json'), 'utf8'));
  // The envelope consists of a 16-byte salt, 12-byte IV and 16-byte GCM tag.
  const plaintextBytes = Buffer.from(wrapper.data, 'base64').length - 44;
  assert.ok(plaintextBytes > 32 * 1024);
  assert.ok(plaintextBytes <= 128 * 1024);
});
