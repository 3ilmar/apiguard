'use strict';

// Uses the pinned, integrity-checked local distribution; never downloads code at runtime.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');

const dependencyDirectory = path.join(__dirname, 'axe');
const scriptPath = path.join(dependencyDirectory, 'axe.min.js');
const tags = Object.freeze(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa']);
const expectedVersion = '4.10.3';

function inspectDependency() {
  try {
    const manifest = JSON.parse(fs.readFileSync(path.join(dependencyDirectory, 'dependency.json'), 'utf8'));
    const packageMetadata = JSON.parse(fs.readFileSync(path.join(dependencyDirectory, 'package.json'), 'utf8'));
    if (manifest.name !== 'axe-core' || manifest.version !== expectedVersion ||
        packageMetadata.name !== 'axe-core' || packageMetadata.version !== expectedVersion ||
        manifest.integrityVerified !== true) {
      throw new Error('The local axe-core distribution does not match the pinned, verified dependency.');
    }
    for (const filename of ['axe.min.js', 'package.json', 'LICENSE']) {
      const digest = crypto.createHash('sha256')
        .update(fs.readFileSync(path.join(dependencyDirectory, filename))).digest('hex');
      if (digest !== manifest.filesSha256[filename]) {
        throw new Error(`Local axe-core file failed SHA-256 validation: ${filename}`);
      }
    }
    return Object.freeze({
      available: true,
      name: manifest.name,
      version: manifest.version,
      source: manifest.source,
      tarball: manifest.tarball,
      integrity: manifest.integrity,
      tarballSha256: manifest.tarballSha256,
      filesSha256: manifest.filesSha256,
      retrievedUtc: manifest.retrievedUtc,
      license: manifest.license,
      tags,
      dependencyError: null,
      limitation: 'Automated axe rules cover only part of WCAG; a scan cannot establish full WCAG conformance.'
    });
  } catch (error) {
    return Object.freeze({
      available: false,
      name: 'axe-core',
      version: expectedVersion,
      tags,
      dependencyError: error.message
    });
  }
}

const availability = inspectDependency();

/**
 * Run axe against the current DOM of a genuine Playwright Page.
 * Returns the unmodified JSON-serializable axe result, including violations,
 * incomplete, passes, inapplicable, node selectors and rule/help metadata.
 * Scanner or page errors reject the call so the caller can report BLOCKED/FAIL.
 */
async function scanPage(page) {
  if (!availability.available) {
    throw new Error(`axe-core dependency unavailable: ${availability.dependencyError}`);
  }
  if (!page || typeof page.addScriptTag !== 'function' || typeof page.evaluate !== 'function') {
    throw new TypeError('scanPage requires a Playwright Page.');
  }
  const injectedScript = await page.addScriptTag({ path: scriptPath });
  await injectedScript.dispose();
  return page.evaluate(async ({ runTags, version }) => {
    if (!globalThis.axe || globalThis.axe.version !== version) {
      throw new Error('The expected pinned axe-core version was not injected into the page.');
    }
    return globalThis.axe.run(document, {
      runOnly: { type: 'tag', values: runTags }
    });
  }, { runTags: tags, version: expectedVersion });
}

module.exports = { scanPage, availability, dependencyError: availability.dependencyError };
