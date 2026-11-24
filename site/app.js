import { navigationTransitions, parseNavigationRoute, resolveNavigationTransition, routeForNavigationState } from "./navigation-fsm.js";

const view = document.querySelector("#view");
const transitions = navigationTransitions;
const routeFor = routeForNavigationState;
const parseRoute = parseNavigationRoute;
let state = parseRoute(location.hash);
let record;
let terrainRecord;
let terrainLoadError;
function send(type, payload={}) {
  const next = resolveNavigationTransition(state.screen, type);
  if (!next) return;
  if (type === "BACK") {
    if (history.state?.previous) { history.back(); return; }
    const destination = state.screen === "FSM" ? (state.returnScreen ?? "HOME") : next;
    state = {...state, ...payload, screen:destination};
    history.replaceState({screen:state.screen,sample:state.sample,terrainMode:state.terrainMode,terrainCell:state.terrainCell}, "", routeFor(state));
    render();
    return;
  }
  const previous = state.screen;
  state = {...state, ...payload, screen:next, ...(type === "INSPECT_FSM" ? {returnScreen:previous} : {}), ...(type === "OPEN_TERRAIN" ? {terrainMode:"edited",terrainCell:144} : {})};
  const route = routeFor(state);
  if (location.hash !== route) history.pushState({screen:state.screen,sample:state.sample,terrainMode:state.terrainMode,terrainCell:state.terrainCell,previous}, "", route);
  render();
}
window.addEventListener("popstate", () => { state = {...parseRoute(location.hash), sample:history.state?.sample ?? 0, terrainMode:history.state?.terrainMode ?? "edited", terrainCell:history.state?.terrainCell ?? 144}; render(); });
window.addEventListener("hashchange", () => {
  const parsed = parseRoute(location.hash);
  if (parsed.screen !== state.screen) { state = parsed; render(); }
});
const fmt = (n, digits=3) => Number(n).toFixed(digits).replace(/(\.\d*?)0+$/,"$1").replace(/\.$/,"");
const vec = (v, unit) => `(${fmt(v.x)}, ${fmt(v.y)}, ${fmt(v.z)}) ${unit}`;
function trajectorySvg() {
  const samples=record.samples, W=900,H=360,L=58,R=28,T=24,B=46;
  const xs=samples.map(s=>s.position.x), zs=samples.map(s=>s.position.z);
  const maxX=Math.max(...xs), minZ=Math.min(0,...zs), maxZ=Math.max(...zs);
  const x=v=>L+(v/(maxX||1))*(W-L-R), y=v=>H-B-(v-minZ)/(maxZ-minZ||1)*(H-T-B);
  const points=samples.map(s=>`${x(s.position.x)},${y(s.position.z)}`).join(" ");
  const ticks=[0,.25,.5,.75,1].map(t=>{const yy=y(minZ+(maxZ-minZ)*t);return `<line x1="${L}" y1="${yy}" x2="${W-R}" y2="${yy}" stroke="#294055"/><text x="${L-12}" y="${yy+4}" fill="#91a9b9" text-anchor="end" font-size="11">${fmt(minZ+(maxZ-minZ)*t,1)}</text>`;}).join("");
  const dots=samples.map((s,i)=>`<circle data-sample="${i}" cx="${x(s.position.x)}" cy="${y(s.position.z)}" r="${i===state.sample?8:5}" fill="${i===state.sample?"#f0c77a":"#7fe0d0"}" stroke="#101b2a" stroke-width="2"><title>Sample ${i}: t=${fmt(s.elapsedTime,2)} s, x=${fmt(s.position.x,2)} m, z=${fmt(s.position.z,2)} m</title></circle>`).join("");
  return `<svg viewBox="0 0 ${W} ${H}" role="img" aria-label="Recorded projectile trajectory: horizontal X position versus vertical Z position">${ticks}<line x1="${L}" y1="${H-B}" x2="${W-R}" y2="${H-B}" stroke="#91a9b9"/><polyline points="${points}" fill="none" stroke="#7fe0d0" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/>${dots}<text x="${(W+L-R)/2}" y="${H-8}" fill="#91a9b9" text-anchor="middle" font-size="12">X position (m)</text><text x="14" y="${H/2}" fill="#91a9b9" transform="rotate(-90 14 ${H/2})" text-anchor="middle" font-size="12">Z position (m)</text></svg>`;
}
function home() {
 const projectilePreview=record?trajectorySvg():`<div class="muted">Projectile evidence is loading.</div>`;
 const terrainPreview=terrainRecord?terrainSvg():`<div class="muted">Terrain evidence is loading or unavailable.</div>`;
 view.innerHTML = `
  <div class="section-head atlas-heading"><div><div class="kicker">Choose a domain</div><h2>Explore the atlas</h2></div><p>Start with a physical question. Every doorway distinguishes implemented experiments from bounded models and research territory.</p></div>
  <div class="grid domain-grid">
    <a class="tile" href="#/experience/projectile" data-action="OPEN_PROJECTILE"><span class="number">01 / MECHANICS</span><h3>Motion & trajectories</h3><p>Follow position and velocity through time. Inspect the actual recorded samples of a 3D projectile experiment.</p><span class="status">● WORKING EXPERIMENT</span></a>
    <a class="tile" href="#/experience/terrain" data-action="OPEN_TERRAIN"><span class="number">02 / GEOLOGY</span><h3>Terrain & landforms</h3><p>Compare a Physics-generated bowl with a smooth elevation edit, then inspect individual grid samples.</p><span class="status">● GENERATED HEIGHT FIELD</span></a>
    <article class="tile"><span class="number">03 / GEOMETRY</span><h3>Shapes & space</h3><p>Explore coordinate frames, spatial relationships, and geometric primitives.</p><span class="status">◌ PRIMITIVES + LESSON RECORDS</span></article>
    <article class="tile"><span class="number">04 / THERMODYNAMICS</span><h3>Heat & energy</h3><p>Trace temperature and energy through bounded thermal models and phase-change examples.</p><span class="status">◌ BOUNDED MODELS</span></article>
    <article class="tile"><span class="number">05 / FLUIDS</span><h3>Water & flow</h3><p>Explore shallow-water fields, rainfall accumulation, and numerical validation evidence.</p><span class="status">◌ BOUNDED MODELS</span></article>
    <article class="tile"><span class="number">06 / ACOUSTICS</span><h3>Waves & sound</h3><p>Compare direct and reflected paths in declared room geometry.</p><span class="status">◌ ANALYTICAL MODELS</span></article>
    <article class="tile"><span class="number">07 / MATERIALS</span><h3>Structure & matter</h3><p>Discover the path from physical properties to stress, deformation, and failure.</p><span class="status">◇ RESEARCH DOMAIN</span></article>
  </div>
  <section class="showcase-section">
   <div class="section-head"><div><div class="kicker">Evidence gallery</div><h2>Inspect the output—not a mockup.</h2></div><p>These previews are rendered from the same checked-in Physics run records used by the interactive experiences below. Each artifact states what it proves and what it does not.</p></div>
   <div class="showcase-grid">
    <article class="showcase-card">
     <div class="showcase-preview trajectory-preview">${projectilePreview}</div>
     <div class="showcase-body"><span class="tag">SOURCE-BACKED RUN RECORD</span><h3>Projectile trajectory</h3><p class="muted">A 3D constant-acceleration experiment projected into X/Z. Select a time sample to inspect the recorded position and velocity.</p><div class="proof-note"><strong>Evidence:</strong> nine samples from the versioned Physics run record, generated and checked by the Pages workflow.</div><div class="limit-note"><strong>Limit:</strong> no drag, collision, terrain impact, or browser-side physics solver.</div><div class="toolbar"><a class="button primary" href="#/experience/projectile" data-action="OPEN_PROJECTILE">Explore experiment →</a><a class="button" href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/experiments/projectile-trajectory.md">Derivation ↗</a></div></div>
    </article>
    <article class="showcase-card">
     <div class="showcase-preview terrain-preview">${terrainPreview}</div>
     <div class="showcase-body"><span class="tag">GENERATED TERRAIN DATA</span><h3>Terrain height field</h3><p class="muted">A 17 × 17 elevation field with an original bowl and a smooth radial edit, visualized directly from recorded values.</p><div class="proof-note"><strong>Evidence:</strong> the .NET generator reproduces both canonical and Pages records; the site lets you select each cell.</div><div class="limit-note"><strong>Limit:</strong> a 2D height field cannot represent caves, overhangs, or geological history.</div><div class="toolbar"><a class="button primary" href="#/experience/terrain" data-action="OPEN_TERRAIN">Explore terrain →</a><a class="button" href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/TERRAIN_HEIGHT_FIELD_2D.md">Model contract ↗</a></div></div>
    </article>
    <article class="showcase-card">
     <div class="showcase-preview acoustic-preview"><img src="./data/acoustic-observation.svg" alt="Source-backed free-field acoustic observation showing a source, listener, five metre separation, propagation delay, and estimated received level"></div>
     <div class="showcase-body"><span class="tag">SOURCE-BACKED ACOUSTIC MODEL</span><h3>Sound arrival & distance</h3><p class="muted">Inspect a declared point source and listener in an ideal free field. The record exposes distance, propagation delay, direction, and estimated sound-pressure level.</p><div class="proof-note"><strong>Evidence:</strong> values are generated by <code>FreeFieldAcoustics.Observe</code> and recorded in a versioned JSON experiment.</div><div class="limit-note"><strong>Limit:</strong> propagation metadata only—not synthesized audio. No reflections, absorption, occlusion, spectrum, or Doppler.</div><div class="toolbar"><a class="button" href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/experiments/acoustic-observation.md">Read experiment & assumptions ↗</a><a class="button" href="./data/acoustic-observation.run.json" download>Download run record ↓</a></div></div>
    </article>
   </div>
  </section>
  <div class="path-grid visitor-paths">
   <article class="path"><span class="kicker">I want to explore</span><h3>Enter a working experiment</h3><p class="muted">Change the selected observation, inspect the model, and follow the result into its evidence record.</p><a href="#/experience/projectile" data-action="OPEN_PROJECTILE">Start with motion →</a></article>
   <article class="path"><span class="kicker">I want to understand</span><h3>Follow theory into code</h3><p class="muted">Read the equations, declared assumptions, source contracts, and tests behind each capability.</p><a href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/tree/development/docs/experiments">Browse experiment notes ↗</a></article>
   <article class="path"><span class="kicker">I want to contribute</span><h3>Trace the evidence boundary</h3><p class="muted">Distinguish implemented, bounded, proposed, and future capabilities before extending the model.</p><a href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/PHYSICS_CAPABILITY_CATALOG.md">Read capability catalog ↗</a></article>
  </div>
  <div class="panel"><div class="kicker">The learning loop</div><h2>Notice → Wonder → Predict → Observe → Explain</h2><p class="muted">A lesson should be approachable at first glance and inspectable all the way down: model identity, declared assumptions, source-backed samples, tests, and limitations.</p><div class="toolbar"><a class="button primary" href="#/experience/projectile" data-action="OPEN_PROJECTILE">Enter the first experiment →</a><a class="button" href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/PHYSICS_CAPABILITY_CATALOG.md">Capability catalog ↗</a><a class="button" href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/GALLERY.md">Documentation gallery ↗</a></div></div>`;
}
function projectile() {
 const s=record.samples[state.sample]??record.samples[0], d=record.definition;
 view.innerHTML=`
 <div class="crumb"><a href="#/home" data-action="BACK">ATLAS</a> / MECHANICS / PROJECTILE TRAJECTORY</div>
 <div class="intro"><div><div class="kicker">Experience 01 · Analytical mechanics</div><h2>Where does the projectile go?</h2><p>Change the selected moment—not the physics—and watch the recorded state reveal the shape of the motion. Every marker below is one of the actual samples emitted by the Physics experiment.</p></div><span class="tag">IMPLEMENTED · MODEL V1</span></div>
 <div class="panel"><div class="section-head"><div><div class="kicker">Recorded trajectory</div><h2>X / Z projection</h2></div><span class="tag">9 SAMPLES · 2.00 s</span></div><div class="plot-wrap">${trajectorySvg()}<div class="plot-legend"><span><i class="dot"></i>Recorded solver samples</span><span style="color:var(--gold)">● Selected sample</span></div></div>
 <label class="muted" for="sample">Inspect time sample <strong id="sample-label">${state.sample} / ${record.samples.length-1}</strong></label><input id="sample" class="range" type="range" min="0" max="${record.samples.length-1}" value="${state.sample}" step="1">
 <div class="stats"><div class="stat"><span>Elapsed time</span><strong>${fmt(s.elapsedTime,2)} s</strong></div><div class="stat"><span>Height · Z</span><strong>${fmt(s.position.z,3)} m</strong></div><div class="stat"><span>Vertical speed</span><strong>${fmt(s.velocity.z,3)} m/s</strong></div><div class="stat"><span>Horizontal · X</span><strong>${fmt(s.position.x,3)} m</strong></div></div>
 <div class="toolbar"><button class="button" id="inspect">Inspect full state →</button><a class="button" href="./data/projectile-trajectory.run.json" download>Download run record ↓</a><a class="button" href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/experiments/projectile-trajectory.md">Read the derivation ↗</a></div></div>
 <div class="inspector"><div class="panel"><div class="kicker">The model</div><h3>Constant acceleration in 3D</h3><div class="equation">p(t) = p₀ + v₀t + ½at²</div><div class="equation">v(t) = v₀ + at</div><p class="muted">Initial velocity ${vec(d.initialState.velocity,"m/s")}; acceleration ${vec(d.acceleration,"m/s²")}. Coordinate frame: ${d.coordinateFrame}.</p></div><div class="panel"><div class="kicker">Assumptions & limits</div><h3>What this model does—and does not—say</h3><p class="muted">This is an ideal point projectile under constant world-space acceleration. It does not include aerodynamic drag, spin, collision detection, terrain intersection, a launch mechanism, or impact response.</p><p class="notice">The curve connects recorded samples as a visual guide. It is not a second physics solver and does not imply real-world flight accuracy.</p></div></div>`;
 document.querySelector("#sample").addEventListener("input",e=>{state={...state,sample:Number(e.target.value)};projectile();});
 document.querySelector("#inspect").addEventListener("click",()=>send("INSPECT"));
}
function inspector() {
 const s=record.samples[state.sample]??record.samples[0];
 view.innerHTML=`
 <div class="crumb"><a href="#/home" data-action="BACK">ATLAS</a> / <a href="#/experience/projectile" data-action="BACK">PROJECTILE</a> / EVIDENCE INSPECTOR</div>
 <div class="intro"><div><div class="kicker">Evidence inspector</div><h2>One state. Every recorded quantity.</h2><p>Sample ${state.sample} is read directly from the checked-in run record. No values are recomputed in this browser view.</p></div><span class="tag">RAW OBSERVATION</span></div>
 <div class="panel"><h3>Sample ${state.sample} · t = ${fmt(s.elapsedTime,3)} s</h3><div class="inspector"><div><div class="kicker">Position · metres</div><p class="equation">${vec(s.position,"m")}</p><div class="kicker">Velocity · metres / second</div><p class="equation">${vec(s.velocity,"m/s")}</p></div><div><div class="kicker">Run definition</div><table class="data-table"><tr><td>Model</td><td>${record.modelId}</td></tr><tr><td>Version</td><td>${record.modelVersion}</td></tr><tr><td>Schema</td><td>${record.schemaVersion}</td></tr><tr><td>Step</td><td>${state.sample} / ${record.definition.stepCount}</td></tr><tr><td>Δt</td><td>${record.definition.deltaTime} s</td></tr><tr><td>Frame</td><td>${record.definition.coordinateFrame}</td></tr></table></div></div></div>
 <div class="panel"><div class="kicker">All samples</div><div style="overflow-x:auto"><table class="data-table"><thead><tr><th>#</th><th>t (s)</th><th>x (m)</th><th>z (m)</th><th>vz (m/s)</th></tr></thead><tbody>${record.samples.map((v,i)=>`<tr data-row="${i}" aria-selected="${i===state.sample}"><td>${i}</td><td>${fmt(v.elapsedTime,2)}</td><td>${fmt(v.position.x,3)}</td><td>${fmt(v.position.z,4)}</td><td>${fmt(v.velocity.z,4)}</td></tr>`).join("")}</tbody></table></div></div>
 <div class="toolbar"><button class="button primary" id="return">← Return to experiment</button><a class="button" href="./data/projectile-trajectory.run.json" download>Download raw JSON ↓</a></div>`;
 document.querySelector("#return").addEventListener("click",()=>send("BACK"));
 document.querySelectorAll("[data-row]").forEach(row=>row.addEventListener("click",()=>{state={...state,sample:Number(row.dataset.row)};render();}));
}
function terrainSvg() {
 const definition=terrainRecord.definition;
 const values=terrainRecord.samples[state.terrainMode==="base"?"baseElevations":"editedElevations"];
 const min=Math.min(...values), max=Math.max(...values), span=max-min||1;
 const halfWidth=18, halfHeight=9, heightScale=2.2, originX=350, originY=62;
 const cells=values.map((value,index)=>{
  const x=index%definition.width, y=Math.floor(index/definition.width);
  const cx=originX+(x-y)*halfWidth;
  const cy=originY+(x+y)*halfHeight-(value-min)*heightScale;
  const t=(value-min)/span, hue=190-(t*145), light=32+(t*17);
  const selected=index===state.terrainCell;
  return `<polygon data-cell="${index}" tabindex="0" role="button" aria-label="Select terrain cell ${x}, ${y}" points="${cx},${cy-8} ${cx+17},${cy} ${cx},${cy+8} ${cx-17},${cy}" fill="hsl(${hue},58%,${light}%)" stroke="${selected?"#f0c77a":"#0d1825"}" stroke-width="${selected?2.5:0.8}"><title>Cell (${x}, ${y}): ${value.toFixed(4)} m</title></polygon>`;
 }).join("");
 return `<svg viewBox="0 0 700 390" role="img" aria-label="Isometric projection of the Physics-generated terrain height samples">${cells}<text x="350" y="378" fill="#91a9b9" text-anchor="middle" font-size="11">17 × 17 recorded elevation samples · X varies fastest</text></svg>`;
}
function terrainProfileSvg() {
 const definition=terrainRecord.definition;
 const values=terrainRecord.samples[state.terrainMode==="base"?"baseElevations":"editedElevations"];
 const row=Math.floor(definition.height/2), line=values.slice(row*definition.width,(row+1)*definition.width);
 const min=Math.min(...line), max=Math.max(...line), span=max-min||1;
 const x=i=>42+(i/(line.length-1))*556, y=v=>122-((v-min)/span)*88;
 const points=line.map((v,i)=>`${x(i)},${y(v)}`).join(" ");
 const dots=line.map((v,i)=>`<circle cx="${x(i)}" cy="${y(v)}" r="3" fill="#7fe0d0"><title>X=${definition.origin.x+i*definition.cellWidth} m, elevation=${v.toFixed(4)} m</title></circle>`).join("");
 return `<svg viewBox="0 0 640 150" role="img" aria-label="Centre-row elevation profile from the selected terrain record"><line x1="42" y1="122" x2="598" y2="122" stroke="#91a9b9"/><polyline points="${points}" fill="none" stroke="#7fe0d0" stroke-width="2.5" stroke-linejoin="round"/>${dots}<text x="42" y="143" fill="#91a9b9" font-size="10">West edge</text><text x="598" y="143" fill="#91a9b9" text-anchor="end" font-size="10">East edge</text><text x="320" y="14" fill="#91a9b9" text-anchor="middle" font-size="10">Centre row · elevation (m)</text></svg>`;
}
function terrain() {
 if(!terrainRecord) {
  view.innerHTML=`<div class="panel"><h2>Terrain record unavailable</h2><p class="muted">${terrainLoadError??"The checked-in terrain record could not be loaded."}</p><p class="muted">The site will not fabricate a replacement terrain field.</p></div>`;
  return;
 }
 const d=terrainRecord.definition;
 const values=terrainRecord.samples[state.terrainMode==="base"?"baseElevations":"editedElevations"];
 const base=terrainRecord.samples.baseElevations;
 const index=Math.max(0,Math.min(values.length-1,state.terrainCell??144));
 const x=index%d.width, y=Math.floor(index/d.width);
 const elevation=values[index], original=base[index];
 const min=Math.min(...values), max=Math.max(...values);
 const worldX=d.origin.x+x*d.cellWidth, worldY=d.origin.y+y*d.cellHeight;
 view.innerHTML=`
 <div class="crumb"><a href="#/home" data-action="BACK">ATLAS</a> / GEOLOGY / TERRAIN HEIGHT FIELD</div>
 <div class="intro"><div><div class="kicker">Experience 02 · Geology / terrain authoring</div><h2>Shape the surface. Inspect the evidence.</h2><p>This is a rendered view of a reproducible Physics height-field record. Switch between the original parabolic bowl and the edited snapshot, then select any grid sample to inspect its actual elevation.</p></div><span class="tag">PHYSICS-GENERATED · 289 SAMPLES</span></div>
 <div class="panel"><div class="section-head"><div><div class="kicker">Recorded terrain surface</div><h2>Isometric height field</h2></div><span class="tag">${d.width} × ${d.height} · ${d.cellWidth} m spacing</span></div>
 <div class="toolbar"><button class="button ${state.terrainMode==="base"?"primary":""}" id="terrain-base">Original bowl</button><button class="button ${state.terrainMode==="edited"?"primary":""}" id="terrain-edited">Brush-edited surface</button><a class="button" href="./data/terrain-height-field.run.json" download>Download run record ↓</a></div>
 <div class="plot-wrap">${terrainSvg()}</div>
 <div class="plot-legend"><span><i class="dot"></i>Lower elevation</span><span style="color:var(--gold)">● Selected sample</span><span>Colour and height are mapped from recorded elevations</span></div></div>
 <div class="inspector"><div class="panel"><div class="kicker">Selected grid sample</div><h3>Cell (${x}, ${y})</h3><div class="stats"><div class="stat"><span>Elevation</span><strong>${elevation.toFixed(3)} m</strong></div><div class="stat"><span>Brush change</span><strong>${(brushDelta>=0?"+":"")+brushDelta.toFixed(3)} m</strong></div><div class="stat"><span>World X</span><strong>${worldX.toFixed(1)} m</strong></div><div class="stat"><span>World Y</span><strong>${worldY.toFixed(1)} m</strong></div></div><p class="muted">Record range: ${min.toFixed(3)}–${max.toFixed(3)} m. Click a diamond in the surface to inspect another sample.</p></div>
 <div class="panel"><div class="kicker">Model and conventions</div><h3>Immutable 2D height field</h3><table class="data-table"><tr><td>Model</td><td>${terrainRecord.modelId}</td></tr><tr><td>Frame</td><td>${d.coordinateFrameId}</td></tr><tr><td>Vertical datum</td><td>${d.verticalDatum}</td></tr><tr><td>Positive vertical</td><td>${d.positiveVerticalDirection}</td></tr><tr><td>Storage</td><td>Row-major · X fastest</td></tr><tr><td>Base</td><td>Parabolic bowl</td></tr><tr><td>Edit</td><td>+2 m smooth radial brush</td></tr></table></div></div>
 <div class="panel"><div class="kicker">Centre-row profile</div><h3>Elevation across the middle of the field</h3><div class="plot-wrap">${terrainProfileSvg()}</div></div>
 <div class="panel"><div class="kicker">Scientific boundary</div><p class="muted">${terrainRecord.limitations.join(" ")}</p><p class="notice">This page visualizes recorded samples from TerrainHeightField2D. It does not run a terrain solver in the browser, infer geological history, or represent caves and overhangs.</p><div class="toolbar"><a class="button" href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/TERRAIN_HEIGHT_FIELD_2D.md">Read the model contract ↗</a><a class="button" href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/tests/TheSingularityWorkshop.Physics.Tests/TerrainHeightField2DTests.cs">Inspect tests ↗</a></div></div>`;
 document.querySelector("#terrain-base").addEventListener("click",()=>{state={...state,terrainMode:"base"};render();});
 document.querySelector("#terrain-edited").addEventListener("click",()=>{state={...state,terrainMode:"edited"};render();});
 document.querySelectorAll("[data-cell]").forEach(cell=>{const selectCell=()=>{state={...state,terrainCell:Number(cell.dataset.cell)};render();};cell.addEventListener("click",selectCell);cell.addEventListener("keydown",event=>{if(event.key==="Enter"||event.key===" "){event.preventDefault();selectCell();}});});
}
function fsmInspector() {
 const rows=Object.entries(transitions).flatMap(([from,actions])=>Object.entries(actions).map(([action,to])=>`<tr><td>${from}</td><td>${action}</td><td>${to==="RETURN"?"Previous screen":to}</td><td><span class="status">${to==="RETURN"?"HISTORY / FALLBACK": "DEFINED TRANSITION"}</span></td></tr>`)).join("");
 const current=state.returnScreen??"HOME";
 view.innerHTML=`
 <div class="crumb"><a href="#/home">ATLAS</a> / SYSTEM / NAVIGATION FSM</div>
 <div class="intro"><div><div class="kicker">Inspectable system</div><h2>Physics Pages navigation machine</h2><p>This is the actual client-side navigation transition map. It governs which screens can be reached and how the browser history returns to the prior screen.</p></div><span class="tag">LIVE FSM DEFINITION</span></div>
 <div class="panel"><div class="section-head"><div><div class="kicker">Current state</div><h2>${state.screen}</h2></div><span class="tag">RETURN TARGET · ${current}</span></div><p class="muted">Route: <code>${location.hash||"#/home"}</code>. The FSM inspector is a read-only view of the transition definition; it does not simulate physical behavior.</p><div class="toolbar"><button class="button primary" id="fsm-back">← Return to previous screen</button><a class="button" href="#/home">Return to atlas</a></div></div>
 <div class="panel"><div class="kicker">Transition table</div><h3>Allowed actions by state</h3><div style="overflow-x:auto"><table class="data-table"><thead><tr><th>From</th><th>Action</th><th>Destination</th><th>Semantics</th></tr></thead><tbody>${rows}</tbody></table></div><p class="notice">Actions absent from this table are rejected by the transition function. BACK uses browser history when a previous in-app state exists; otherwise it uses the declared fallback.</p></div>
 <div class="panel"><div class="kicker">Architecture boundary</div><p class="muted">This machine controls presentation navigation only. Projectile measurements remain sourced from the checked-in Physics run record; this UI state machine is not a Physics solver or experiment executor.</p></div>`;
 document.querySelector("#fsm-back").addEventListener("click",()=>send("BACK"));
}
function render(){
 document.querySelectorAll("nav [data-route]").forEach(a=>a.toggleAttribute("aria-current", (state.screen==="HOME"&&a.dataset.route==="home")||((state.screen==="PROJECTILE"||state.screen==="INSPECTOR"||state.screen==="TERRAIN")&&a.dataset.route==="experience")||(state.screen==="FSM"&&a.dataset.route==="system")));
 if(state.screen==="HOME") home(); else if(state.screen==="PROJECTILE") projectile(); else if(state.screen==="INSPECTOR") inspector(); else if(state.screen==="TERRAIN") terrain(); else fsmInspector();
}
document.addEventListener("click",e=>{
 const action=e.target.closest("[data-action]")?.dataset.action;
 if(action==="OPEN_PROJECTILE"){e.preventDefault();send(action);}
 if(action==="OPEN_TERRAIN"){e.preventDefault();send(action);}
 if(action==="INSPECT_FSM"){e.preventDefault();send(action);}
 if(action==="BACK"){e.preventDefault();send(action);}
});
async function start(){
 try {
  const response=await fetch("./data/projectile-trajectory.run.json");
  if(!response.ok) throw new Error(`Run record request failed: ${response.status}`);
  record=await response.json();
  if(!record.modelId||!Array.isArray(record.samples)||record.samples.length!==record.definition.stepCount+1) throw new Error("The run record failed its structural checks.");
  try {
   const terrainResponse=await fetch("./data/terrain-height-field.run.json");
   if(!terrainResponse.ok) throw new Error(`Terrain record request failed: ${terrainResponse.status}`);
   const loadedTerrain=await terrainResponse.json();
   const expectedCells=loadedTerrain.definition.width*loadedTerrain.definition.height;
   if(!loadedTerrain.modelId||loadedTerrain.samples.baseElevations.length!==expectedCells||loadedTerrain.samples.editedElevations.length!==expectedCells) throw new Error("The terrain record failed its structural checks.");
   terrainRecord=loadedTerrain;
  } catch(error) { terrainLoadError=String(error.message); }
  render();
 } catch(error) {
  view.innerHTML=`<div class="panel"><h2>Physics data unavailable</h2><p class="muted">${String(error.message)}</p><p class="muted">The site will not invent replacement measurements. Check the published run-record artifact and try again.</p></div>`;
 }
}
start();
