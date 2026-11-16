// Draws the Report 4 diagrams as SVG in the style of the school's samples (white boxes, light shadow, Arial).
// Runs inside the page that diagrams.mjs opens in headless Chrome; text is measured with a canvas so boxes fit.
window.R = (() => {
  const FONT = 'Arial, Helvetica, sans-serif'
  const ctx = document.createElement('canvas').getContext('2d')
  const tw = (s, size = 12, bold = false, italic = false) => {
    ctx.font = `${italic ? 'italic ' : ''}${bold ? 'bold ' : ''}${size}px Arial`
    return ctx.measureText(String(s)).width
  }
  const esc = (s) => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
  const T = (x, y, s, o = {}) =>
    `<text x="${x}" y="${y}" font-family="${FONT}" font-size="${o.size || 12}"` +
    `${o.bold ? ' font-weight="bold"' : ''}${o.italic ? ' font-style="italic"' : ''}` +
    ` text-anchor="${o.anchor || 'start'}" fill="${o.fill || '#000'}">${esc(s)}</text>`
  const pts = (a) => a.map((p) => p.join(',')).join(' ')
  const SHADOW = '#c9c9c9'

  // ----- geometry ------------------------------------------------------------------------------------------------
  const center = (r) => [r.x + r.w / 2, r.y + r.h / 2]
  function clip(r, p) {
    const [cx, cy] = center(r)
    const dx = p[0] - cx
    const dy = p[1] - cy
    if (!dx && !dy) return [cx, cy]
    const sx = dx ? r.w / 2 / Math.abs(dx) : Infinity
    const sy = dy ? r.h / 2 / Math.abs(dy) : Infinity
    const s = Math.min(sx, sy)
    return [cx + dx * s, cy + dy * s]
  }
  // Anchor = [fx, fy] fractions of the box, e.g. [0.5, 1] = middle of the bottom edge.
  const at = (r, a) => [r.x + a[0] * r.w, r.y + a[1] * r.h]

  /** Arrow head at `tip` coming from `from`. Returns the point where the line should stop. */
  function head(kind, tip, from, out) {
    const dx = tip[0] - from[0]
    const dy = tip[1] - from[1]
    const L = Math.hypot(dx, dy) || 1
    const ux = dx / L
    const uy = dy / L
    const P = (a, b) => [tip[0] - ux * a - uy * b, tip[1] - uy * a + ux * b]
    switch (kind) {
      case 'tri':
        out.push(`<polygon points="${pts([tip, P(15, 8), P(15, -8)])}" fill="#fff" stroke="#000"/>`)
        return P(15, 0)
      case 'open':
        out.push(`<polyline points="${pts([P(11, 6), tip, P(11, -6)])}" fill="none" stroke="#000"/>`)
        return tip
      case 'fill':
        out.push(`<polygon points="${pts([tip, P(11, 5), P(11, -5)])}" fill="#000"/>`)
        return P(10, 0)
      case 'diamond':
      case 'fdiamond':
        out.push(`<polygon points="${pts([tip, P(9, 5), P(18, 0), P(9, -5)])}" fill="${kind === 'fdiamond' ? '#000' : '#fff'}" stroke="#000"/>`)
        return P(18, 0)
      case 'many': // crow's foot
        out.push(`<polyline points="${pts([P(0, 7), P(12, 0), P(0, -7)])}" fill="none" stroke="#000"/>`)
        out.push(`<line x1="${P(16, 6)[0]}" y1="${P(16, 6)[1]}" x2="${P(16, -6)[0]}" y2="${P(16, -6)[1]}" stroke="#000"/>`)
        return tip
      case 'one':
        for (const d of [8, 13]) out.push(`<line x1="${P(d, 6)[0]}" y1="${P(d, 6)[1]}" x2="${P(d, -6)[0]}" y2="${P(d, -6)[1]}" stroke="#000"/>`)
        return tip
      case 'zero-one':
        out.push(`<line x1="${P(8, 6)[0]}" y1="${P(8, 6)[1]}" x2="${P(8, -6)[0]}" y2="${P(8, -6)[1]}" stroke="#000"/>`)
        out.push(`<circle cx="${P(17, 0)[0]}" cy="${P(17, 0)[1]}" r="4.5" fill="#fff" stroke="#000"/>`)
        return tip
      default:
        return tip
    }
  }

  // Edge kinds: extends / implements / uses / assoc / aggr / compose / rel (ERD) / flow (block diagrams).
  const EDGE = {
    extends: { end: 'tri' },
    implements: { end: 'tri', dash: true },
    uses: { end: 'open', dash: true },
    assoc: { end: 'open' },
    aggr: { start: 'diamond', end: 'open' },
    compose: { start: 'fdiamond' },
    flow: { end: 'fill' },
    dep: { end: 'open', dash: true },
  }

  function edge(e, boxes, out, labels) {
    const A = boxes[e.a]
    const B = boxes[e.b]
    if (!A || !B) throw new Error('edge ' + e.a + ' -> ' + e.b)
    const via = e.via || []
    const p0 = e.ap ? at(A, e.ap) : clip(A, via[0] || center(B))
    const pn = e.bp ? at(B, e.bp) : clip(B, via[via.length - 1] || center(A))
    const line = [p0, ...via, pn]
    const st = e.kind === 'rel' ? { start: e.from || 'one', end: e.to || 'many' } : EDGE[e.kind] || {}
    const heads = []
    if (st.end) line[line.length - 1] = head(st.end, pn, line[line.length - 2], heads)
    if (st.start) line[0] = head(st.start, p0, line[1], heads)
    out.push(`<polyline points="${pts(line)}" fill="none" stroke="#000"${st.dash || e.dash ? ' stroke-dasharray="6,4"' : ''}/>`)
    out.push(...heads)
    if (e.label) {
      // On the longest segment, or where the spec says.
      let best = 0
      let bi = 0
      for (let i = 0; i < line.length - 1; i++) {
        const l = Math.hypot(line[i + 1][0] - line[i][0], line[i + 1][1] - line[i][1])
        if (l > best) [best, bi] = [l, i]
      }
      const m = e.lp || [(line[bi][0] + line[bi + 1][0]) / 2, (line[bi][1] + line[bi + 1][1]) / 2]
      const parts = String(e.label).split('\n')
      const w = Math.max(...parts.map((p) => tw(p, 11))) + 8
      const h = parts.length * 13 + 4
      labels.push(`<rect x="${m[0] - w / 2}" y="${m[1] - h / 2}" width="${w}" height="${h}" fill="#fff"/>`)
      parts.forEach((p, i) => labels.push(T(m[0], m[1] - h / 2 + 13 * (i + 1) - 1, p, { size: 11, anchor: 'middle', italic: e.italic })))
    }
  }

  // ----- class diagram / ERD -------------------------------------------------------------------------------------
  const LINE = 15
  function classBox(n, out) {
    const head = n.stereo ? 36 : 24
    const nameOpt = { bold: true, italic: n.stereo === 'interface' || n.abstract }
    const w = Math.max(
      n.minW || 100,
      tw(n.name, 12, true) + 20,
      n.stereo ? tw(`«${n.stereo}»`, 11) + 20 : 0,
      ...(n.attrs || []).map((a) => tw(a, 11) + 16),
      ...(n.ops || []).map((o) => tw(o, 11) + 16),
    )
    const aH = n.attrs?.length ? n.attrs.length * LINE + 8 : 8
    const oH = n.ops?.length ? n.ops.length * LINE + 8 : 8
    const h = head + aH + (n.noOps ? 0 : oH)
    const { x, y } = n
    out.push(`<rect x="${x + 4}" y="${y + 4}" width="${w}" height="${h}" fill="${SHADOW}"/>`)
    out.push(`<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="${n.fill || '#fff'}" stroke="#000"/>`)
    if (n.stereo) out.push(T(x + w / 2, y + 14, `«${n.stereo}»`, { size: 11, anchor: 'middle' }))
    out.push(T(x + w / 2, y + head - 8, n.name, { ...nameOpt, anchor: 'middle' }))
    out.push(`<line x1="${x}" y1="${y + head}" x2="${x + w}" y2="${y + head}" stroke="#000"/>`)
    ;(n.attrs || []).forEach((a, i) => out.push(T(x + 7, y + head + 4 + LINE * (i + 1) - 3, a, { size: 11 })))
    if (!n.noOps) {
      out.push(`<line x1="${x}" y1="${y + head + aH}" x2="${x + w}" y2="${y + head + aH}" stroke="#000"/>`)
      ;(n.ops || []).forEach((o, i) => out.push(T(x + 7, y + head + aH + 4 + LINE * (i + 1) - 3, o, { size: 11 })))
    }
    return { x, y, w, h }
  }

  // ERD table: header with the table name, then "PK/FK  column  type".
  function tableBox(n, out) {
    const cols = n.cols.map((c) => {
      const [key, name, type] = c.split('|')
      return { key: key.trim(), name: name.trim(), type: (type || '').trim() }
    })
    const nameW = Math.max(...cols.map((c) => tw(c.name, 11, c.key.includes('PK'))))
    const typeW = Math.max(...cols.map((c) => tw(c.type, 10)))
    const w = Math.max(tw(n.name, 12, true) + 24, 34 + nameW + 14 + typeW + 10)
    const head = 24
    const h = head + cols.length * 15 + 8
    const { x, y } = n
    out.push(`<rect x="${x + 4}" y="${y + 4}" width="${w}" height="${h}" fill="${SHADOW}"/>`)
    out.push(`<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="#fff" stroke="#000"/>`)
    out.push(`<rect x="${x}" y="${y}" width="${w}" height="${head}" fill="${n.fill || '#e6e6e6'}" stroke="#000"/>`)
    out.push(T(x + w / 2, y + 16, n.name, { bold: true, anchor: 'middle' }))
    out.push(`<line x1="${x + 30}" y1="${y + head}" x2="${x + 30}" y2="${y + h}" stroke="#bbb"/>`)
    cols.forEach((c, i) => {
      const ty = y + head + 4 + 15 * (i + 1) - 3
      if (c.key) out.push(T(x + 15, ty, c.key, { size: 9, bold: true, anchor: 'middle' }))
      out.push(T(x + 36, ty, c.name, { size: 11, bold: c.key.includes('PK') }))
      out.push(T(x + w - 6, ty, c.type, { size: 10, anchor: 'end', fill: '#555' }))
    })
    return { x, y, w, h }
  }

  // ----- block diagrams (architecture, packages) ----------------------------------------------------------------
  function block(n, out) {
    const { x, y, w, h } = n
    const lines = n.lines || []
    switch (n.shape) {
      case 'group':
        out.push(`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="8" fill="${n.fill || '#fafafa'}" stroke="#666" stroke-dasharray="7,4"/>`)
        out.push(T(x + 12, y + 20, n.title, { bold: true, size: 13 }))
        lines.forEach((l, i) => out.push(T(x + 12, y + 37 + i * 14, l, { size: 11, fill: '#444' })))
        break
      case 'pkg': {
        const tabW = tw(n.title, 12, true) + 20
        out.push(`<rect x="${x + 4}" y="${y + 22}" width="${w}" height="${h - 18}" fill="${SHADOW}"/>`)
        out.push(`<rect x="${x}" y="${y}" width="${tabW}" height="20" fill="${n.fill || '#fff'}" stroke="#000"/>`)
        out.push(`<rect x="${x}" y="${y + 20}" width="${w}" height="${h - 20}" fill="${n.fill || '#fff'}" stroke="#000"/>`)
        out.push(T(x + 10, y + 15, n.title, { bold: true }))
        lines.forEach((l, i) => out.push(T(x + 10, y + 40 + i * 15, l, { size: 11 })))
        break
      }
      case 'db': {
        const ry = 10
        out.push(`<path d="M${x},${y + ry} v${h - 2 * ry} a${w / 2},${ry} 0 0 0 ${w},0 v${-(h - 2 * ry)}" fill="${n.fill || '#fff'}" stroke="#000"/>`)
        out.push(`<ellipse cx="${x + w / 2}" cy="${y + ry}" rx="${w / 2}" ry="${ry}" fill="${n.fill || '#fff'}" stroke="#000"/>`)
        out.push(T(x + w / 2, y + 2 * ry + 18, n.title, { bold: true, anchor: 'middle' }))
        lines.forEach((l, i) => out.push(T(x + w / 2, y + 2 * ry + 34 + i * 14, l, { size: 11, anchor: 'middle' })))
        break
      }
      case 'actor': {
        const cx = x + w / 2
        out.push(`<circle cx="${cx}" cy="${y + 10}" r="9" fill="#fff" stroke="#000"/>`)
        out.push(`<path d="M${cx},${y + 19} v20 M${cx - 13},${y + 26} h26 M${cx},${y + 39} l-11,15 M${cx},${y + 39} l11,15" fill="none" stroke="#000"/>`)
        out.push(T(cx, y + 70, n.title, { bold: true, anchor: 'middle' }))
        lines.forEach((l, i) => out.push(T(cx, y + 85 + i * 14, l, { size: 11, anchor: 'middle' })))
        break
      }
      default: {
        out.push(`<rect x="${x + 4}" y="${y + 4}" width="${w}" height="${h}" rx="6" fill="${SHADOW}"/>`)
        out.push(`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="6" fill="${n.fill || '#fff'}" stroke="#000"/>`)
        const top = y + h / 2 - ((lines.length + 1) * 14) / 2 + 10
        out.push(T(x + w / 2, top, n.title, { bold: true, anchor: 'middle' }))
        lines.forEach((l, i) => out.push(T(x + w / 2, top + 16 + i * 14, l, { size: 11, anchor: 'middle', fill: '#333' })))
      }
    }
    return { x, y, w, h }
  }

  function graph(spec) {
    const shapes = []
    const boxes = {}
    // Groups first so the boxes inside sit on top of them; edges over boxes like the samples.
    const ordered = [...spec.nodes].sort((a, b) => (b.shape === 'group') - (a.shape === 'group'))
    for (const n of ordered) {
      const draw = spec.type === 'erd' ? tableBox : spec.type === 'block' || n.shape ? block : classBox
      boxes[n.id] = draw(n, shapes)
    }
    const lines = []
    const labels = []
    for (const e of spec.edges || []) edge(e, boxes, lines, labels)
    return finish([...shapes, ...lines, ...labels], spec)
  }

  // ----- sequence diagram ----------------------------------------------------------------------------------------
  function seq(spec) {
    const P = spec.parts
    const ix = Object.fromEntries(P.map((p, i) => [p.id, i]))
    const pw = P.map((p) => Math.max(p.actor ? 70 : 90, tw(p.name, 12, true) + (p.actor ? 10 : 26)))
    const msgs = []
    const walk = (steps) => {
      for (const s of steps) {
        if (s.alt) s.alt.forEach(([, st]) => walk(st))
        else if (s.m || s.r) msgs.push(s.m || s.r)
      }
    }
    walk(spec.steps)
    const gap = P.slice(1).map((_, i) => Math.max((pw[i] + pw[i + 1]) / 2 + 24, 110))
    let rightExtra = 30
    for (const [f, t, text] of [...msgs].sort((a, b) => Math.abs(ix[a[0]] - ix[a[1]]) - Math.abs(ix[b[0]] - ix[b[1]]))) {
      const a = Math.min(ix[f], ix[t])
      const b = Math.max(ix[f], ix[t])
      const need = tw('00 : ' + text, 12) + 36
      if (a === b) {
        if (a < P.length - 1) gap[a] = Math.max(gap[a], need + 24)
        else rightExtra = Math.max(rightExtra, need + 20)
        continue
      }
      let cur = 0
      for (let i = a; i < b; i++) cur += gap[i]
      if (cur < need) for (let i = a; i < b; i++) gap[i] += (need - cur) / (b - a)
    }
    const X = [Math.max(pw[0] / 2, 50) + 14]
    gap.forEach((g, i) => X.push(X[i] + g))

    const TOP = 10
    const HEAD = 44
    const bars = []
    const frames = []
    const msgOut = []
    const stack = P.map(() => [])
    const BW = 12
    const barL = (i, d) => X[i] - BW / 2 + (d - 1) * 6
    const edgeX = (i, dir, d = stack[i].length) => (d === 0 ? X[i] : dir > 0 ? barL(i, d) + BW : barL(i, d))
    let y = TOP + HEAD + 34
    let n = 1

    const label = (x1, x2, yy, text) => msgOut.push(T((x1 + x2) / 2, yy - 6, text, { anchor: 'middle' }))
    function run(steps) {
      for (const s of steps) {
        if (s.alt) {
          const involved = new Set()
          const coll = (st) => st.forEach((q) => (q.alt ? q.alt.forEach(([, z]) => coll(z)) : (q.m || q.r) && [q.m || q.r][0].slice(0, 2).forEach((p) => involved.add(ix[p]))))
          s.alt.forEach(([, st]) => coll(st))
          const lo = Math.min(...involved)
          const hi = Math.max(...involved)
          const fx = X[lo] - Math.max(40, pw[lo] / 2 - 10)
          const fw = X[hi] + 40 - fx + (s.wide || 0)
          const snap = stack.map((a) => [...a])
          const top = y - 22
          const guards = []
          s.alt.forEach(([guard, st], bi) => {
            if (bi > 0) {
              y += 2
              frames.push(`<line x1="${fx}" y1="${y - 20}" x2="${fx + fw}" y2="${y - 20}" stroke="#000" stroke-dasharray="6,4"/>`)
              y += 14
              for (let i = 0; i < P.length; i++) stack[i] = [...snap[i]]
            }
            const gx = fx + (bi === 0 ? 48 : 6)
            const gy = (bi === 0 ? top : y - 34) + 15
            const gw = tw(`[${guard}]`, 11, true) + 6
            guards.push(`<rect x="${gx - 3}" y="${gy - 11}" width="${gw}" height="14" fill="#fff"/>`)
            guards.push(T(gx, gy, `[${guard}]`, { size: 11, bold: true }))
            y += bi === 0 ? 22 : 10
            run(st)
            if (bi < s.alt.length - 1) {
              // Bars opened inside this branch end with it.
              for (let i = 0; i < P.length; i++) {
                while (stack[i].length > snap[i].length) bars.push({ i, d: stack[i].length, y1: stack[i].pop(), y2: y - 18 })
              }
            }
          })
          const bottom = y - 14
          frames.push(`<rect x="${fx}" y="${top}" width="${fw}" height="${bottom - top}" fill="none" stroke="#000"/>`)
          frames.push(`<path d="M${fx},${top} h40 v12 l-6,7 h-34 z" fill="#fff" stroke="#000"/>`)
          frames.push(T(fx + 6, top + 14, 'alt', { bold: true, size: 11 }))
          msgOut.push(...guards)
          y += 14
          continue
        }
        if (s.m) {
          const [f, t, text] = s.m
          const a = ix[f]
          const b = ix[t]
          if (a === b) {
            const d = stack[a].length
            const x1 = edgeX(a, 1)
            msgOut.push(`<polyline points="${pts([[x1, y - 8], [x1 + 34, y - 8], [x1 + 34, y + 8]])}" fill="none" stroke="#000"/>`)
            const nx = barL(a, d + 1) + BW
            const hp = []
            head('fill', [nx, y + 8], [x1 + 34, y + 8], hp)
            msgOut.push(`<line x1="${x1 + 34}" y1="${y + 8}" x2="${nx + 10}" y2="${y + 8}" stroke="#000"/>`, ...hp)
            bars.push({ i: a, d: d + 1, y1: y + 2, y2: y + 22 })
            msgOut.push(T(x1 + 40, y + 2, `${n++} : ${text}`))
            y += 40
            continue
          }
          const dir = Math.sign(b - a)
          const x1 = edgeX(a, dir)
          stack[b].push(y - 4)
          const x2 = edgeX(b, -dir)
          const hp = []
          const end = head('fill', [x2, y], [x1, y], hp)
          msgOut.push(`<line x1="${x1}" y1="${y}" x2="${end[0]}" y2="${y}" stroke="#000"/>`, ...hp)
          label(x1, x2, y, `${n++} : ${text}`)
          y += 32
        } else if (s.r) {
          const [f, t, text] = s.r
          const a = ix[f]
          const b = ix[t]
          const dir = Math.sign(b - a)
          const x1 = edgeX(a, dir)
          const d = stack[a].length
          if (d) bars.push({ i: a, d, y1: stack[a].pop(), y2: y + 4 })
          const x2 = edgeX(b, -dir)
          const hp = []
          head('open', [x2, y], [x1, y], hp)
          msgOut.push(`<line x1="${x1}" y1="${y}" x2="${x2}" y2="${y}" stroke="#000" stroke-dasharray="5,4"/>`, ...hp)
          label(x1, x2, y, `${n++} : ${text}`)
          y += 32
        }
      }
    }
    run(spec.steps)
    for (let i = 0; i < P.length; i++) while (stack[i].length) bars.push({ i, d: stack[i].length, y1: stack[i].pop(), y2: y - 10 })
    const endY = y + 4

    const out = []
    P.forEach((p, i) => {
      out.push(`<line x1="${X[i]}" y1="${TOP + HEAD}" x2="${X[i]}" y2="${endY}" stroke="#000" stroke-dasharray="4,4"/>`)
      if (p.actor) {
        const cx = X[i]
        out.push(`<circle cx="${cx}" cy="${TOP + 7}" r="7" fill="#fff" stroke="#000"/>`)
        out.push(`<path d="M${cx},${TOP + 14} v14 M${cx - 10},${TOP + 19} h20 M${cx},${TOP + 28} l-8,10 M${cx},${TOP + 28} l8,10" fill="none" stroke="#000"/>`)
        out.push(`<rect x="${cx - tw(p.name, 12, true) / 2 - 3}" y="${TOP + 39}" width="${tw(p.name, 12, true) + 6}" height="15" fill="#fff"/>`)
        out.push(T(cx, TOP + 51, p.name, { bold: true, anchor: 'middle' }))
      } else {
        const w = pw[i]
        out.push(`<rect x="${X[i] - w / 2 + 4}" y="${TOP + 4}" width="${w}" height="${HEAD - 6}" fill="${SHADOW}"/>`)
        out.push(`<rect x="${X[i] - w / 2}" y="${TOP}" width="${w}" height="${HEAD - 6}" fill="#fff" stroke="#000"/>`)
        out.push(T(X[i], TOP + 23, p.name, { bold: true, anchor: 'middle' }))
      }
    })
    bars.sort((p, q) => p.d - q.d)
    for (const b of bars) out.push(`<rect x="${barL(b.i, b.d)}" y="${b.y1}" width="${BW}" height="${Math.max(b.y2 - b.y1, 8)}" fill="#fff" stroke="#000"/>`)
    const w = X[X.length - 1] + Math.max(pw[pw.length - 1] / 2, rightExtra) + 16
    return finish([...frames, ...out.slice(0), ...msgOut], { width: w, height: endY + 12 })
  }

  function finish(parts, spec) {
    const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg')
    svg.innerHTML = parts.join('')
    document.body.innerHTML = ''
    document.body.appendChild(svg)
    let { width, height } = spec
    if (!width || !height) {
      const bb = svg.getBBox()
      width = Math.ceil(bb.x + bb.width + 16)
      height = Math.ceil(bb.y + bb.height + 16)
    }
    width = Math.ceil(width)
    height = Math.ceil(height)
    svg.setAttribute('width', width)
    svg.setAttribute('height', height)
    svg.setAttribute('viewBox', `0 0 ${width} ${height}`)
    return { width, height }
  }

  return { render: (spec) => (spec.type === 'seq' ? seq(spec) : graph(spec)) }
})()
