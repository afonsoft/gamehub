/**
 * Postinstall patch for Angular 15's devtools-ignore-plugin.js
 *
 * Angular 15's DevToolsIgnorePlugin crashes when a source-map file
 * starts with a UTF-8 BOM (\uFEFF) or lacks a "sources" array.
 * This is a known issue that was never fixed in the 15.x line.
 *
 * This script patches the plugin to:
 *   1. Strip the BOM before calling JSON.parse()
 *   2. Wrap JSON.parse in try/catch for malformed source maps
 *   3. Skip source maps that have no "sources" array
 */
const fs = require('node:fs');
const path = require('node:path');

const pluginPath = path.join(
  __dirname,
  '..',
  'node_modules',
  '@angular-devkit',
  'build-angular',
  'src',
  'webpack',
  'plugins',
  'devtools-ignore-plugin.js'
);

if (!fs.existsSync(pluginPath)) {
  console.log('[patch] devtools-ignore-plugin.js not found, skipping.');
  process.exit(0);
}

let content = fs.readFileSync(pluginPath, 'utf8');

// Already fully patched
if (content.includes('PATCHED_BOM_GUARD')) {
  console.log('[patch] devtools-ignore-plugin.js already patched.');
  process.exit(0);
}

// Match the original unpatched block
const original =
  '                    const mapContent = asset.source().toString();\n' +
  '                    if (!mapContent) {\n' +
  '                        continue;\n' +
  '                    }\n' +
  '                    const map = JSON.parse(mapContent);\n' +
  '                    const ignoreList = [];\n' +
  '                    for (const [index, path] of map.sources.entries()) {';

// Also match if the BOM-only patch was already applied
const bomOnlyPatched =
  '                    const mapContent = asset.source().toString();\n' +
  '                    if (!mapContent) {\n' +
  '                        continue;\n' +
  '                    }\n' +
  "                    const map = JSON.parse(mapContent.replace(/^\\uFEFF/, ''));\n" +
  '                    const ignoreList = [];\n' +
  '                    for (const [index, path] of map.sources.entries()) {';

const replacement =
  '                    // PATCHED_BOM_GUARD: strip BOM, guard parse & missing sources\n' +
  '                    let mapContent = asset.source().toString();\n' +
  '                    if (!mapContent) {\n' +
  '                        continue;\n' +
  '                    }\n' +
  '                    if (mapContent.charCodeAt(0) === 0xFEFF) {\n' +
  '                        mapContent = mapContent.slice(1);\n' +
  '                    }\n' +
  '                    let map;\n' +
  '                    try { map = JSON.parse(mapContent); } catch (_e) { continue; }\n' +
  '                    if (!map.sources || !Array.isArray(map.sources)) { continue; }\n' +
  '                    const ignoreList = [];\n' +
  '                    for (const [index, path] of map.sources.entries()) {';

if (content.includes(original)) {
  content = content.replace(original, replacement);
} else if (content.includes(bomOnlyPatched)) {
  content = content.replace(bomOnlyPatched, replacement);
} else {
  console.log('[patch] Could not find target block in devtools-ignore-plugin.js, skipping.');
  process.exit(0);
}

fs.writeFileSync(pluginPath, content, 'utf8');
console.log('[patch] devtools-ignore-plugin.js patched (BOM + missing sources guard).');
