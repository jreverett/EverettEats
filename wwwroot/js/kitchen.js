// Client-side reader for unlisted draft recipes that are stored encrypted.
// The recipe text never lives in the repo as plain HTML, and the encrypted blob
// is fetched and unlocked in the browser. The result is returned as a byte
// stream, so Blazor can transfer bounded chunks rather than one large message.
window.kitchenOpen = async function (url, passphrase) {
  if (!globalThis.crypto || !crypto.subtle) {
    throw new Error('Web Crypto needs a secure context — open over https or http://localhost (not 0.0.0.0 or a LAN IP).');
  }
  const pass = passphrase.trim().toLowerCase().replace(/[^a-z0-9]/g, '');
  const res = await fetch(url, { cache: 'no-store' });
  if (!res.ok) {
    throw new Error('not found');
  }
  const wrapper = await res.json();
  const raw = Uint8Array.from(atob(wrapper.data), c => c.charCodeAt(0));
  const salt = raw.slice(0, 16), iv = raw.slice(16, 28), data = raw.slice(28);
  const km = await crypto.subtle.importKey('raw', new TextEncoder().encode(pass), 'PBKDF2', false, ['deriveKey']);
  const key = await crypto.subtle.deriveKey(
    { name: 'PBKDF2', salt, iterations: 150000, hash: 'SHA-256' },
    km, { name: 'AES-GCM', length: 256 }, false, ['decrypt']);
  const pt = await crypto.subtle.decrypt({ name: 'AES-GCM', iv }, key, data);
  return new Uint8Array(pt);
};
