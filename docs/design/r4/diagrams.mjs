// Renders every Report 4 diagram in specs/*.mjs to img/<name>.png with the Chrome that the taste skill already uses.
// Usage (from the repo root): node docs/design/r4/diagrams.mjs [name ...]
import { mkdirSync, readFileSync, writeFileSync } from 'node:fs'
import { dirname, join } from 'node:path'
import { fileURLToPath, pathToFileURL } from 'node:url'
import { Browser } from '../../../.claude/skills/design-taste-frontend/scripts/lib/cdp.mjs'

const here = dirname(fileURLToPath(import.meta.url))
const only = process.argv.slice(2)
const specs = {}
for (const file of ['system.mjs', 'auth.mjs', 'cases.mjs']) {
  Object.assign(specs, (await import(pathToFileURL(join(here, 'specs', file)).href)).default)
}

const out = join(here, 'img')
mkdirSync(out, { recursive: true })
const sizesFile = join(out, 'sizes.json')
let sizes = {}
try {
  sizes = JSON.parse(readFileSync(sizesFile, 'utf8'))
} catch {}

const browser = await Browser.launch()
try {
  const page = await browser.newPage({ width: 1600, height: 1200, dsf: 2 })
  await page.goto('about:blank', { settleMs: 0 })
  await page.eval(`document.body.style.margin = '0'; document.body.style.background = '#fff'`)
  await page.eval(readFileSync(join(here, 'render.js'), 'utf8'))
  for (const [name, spec] of Object.entries(specs)) {
    if (only.length && !only.includes(name)) continue
    const size = await page.eval((s) => window.R.render(s), spec)
    if (!(size.width > 0 && size.height > 0)) throw new Error(name + " bad size " + JSON.stringify(size))
    await page.setViewport({ width: Math.max(size.width, 200), height: Math.max(size.height, 200), dsf: 2 })
    await page.screenshot({ path: join(out, `${name}.png`), format: 'png', clip: { x: 0, y: 0, width: size.width, height: size.height } })
    sizes[name] = size
    console.log(`${name}.png  ${size.width}x${size.height}`)
  }
} finally {
  await browser.close()
}
writeFileSync(sizesFile, JSON.stringify(sizes, null, 2))
