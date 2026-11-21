const view = document.querySelector("#view");
const transitions = {
  HOME: { OPEN_PROJECTILE: "PROJECTILE" },
  PROJECTILE: { BACK: "HOME", INSPECT: "INSPECTOR", SELECT_SAMPLE: "PROJECTILE" },
  INSPECTOR: { BACK: "PROJECTILE", SELECT_SAMPLE: "INSPECTOR" }
};
const routeFor = state => state.screen === "HOME" ? "#/home" : state.screen === "PROJECTILE" ? "#/experience/projectile" : "#/experience/projectile/evidence";
function parseRoute(hash) {
  if (hash === "#/experience/projectile/evidence") return {screen:"INSPECTOR", sample:0};
  if (hash === "#/experience/projectile") return {screen:"PROJECTILE", sample:0};
  return {screen:"HOME", sample:0};
}
let state = parseRoute(location.hash);
let record;
function send(type, payload={}) {
  const next = transitions[state.screen]?.[type];
  if (!next) return;
  state = {...state, ...payload, screen:next};
  const route = routeFor(state);
  if (location.hash !== route) history.pushState({screen:state.screen,sample:state.sample}, "", route);
  render();
}
window.addEventListener("popstate", () => { state = parseRoute(location.hash); render(); });
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
  view.innerHTML = `
  <div class="section-head"><div><div class="kicker">Choose a domain</div><h2>Explore the atlas</h2></div><p>Each doorway leads to a physical question. The status tells you whether the model is implemented, bounded, or still a research direction.</p></div>
  <div class="grid">
    <a class="tile" href="#/experience/projectile" data-action="OPEN_PROJECTILE"><span class="number">01 / MECHANICS</span><h3>Motion & trajectories</h3><p>Follow position and velocity through time. Inspect a recorded 3D projectile experiment.</p><span class="status">● WORKING EXPERIMENT</span></a>
    <article class="tile"><span class="number">02 / GEOMETRY</span><h3>Shapes & space</h3><p>Explore boundaries, coordinates, transformations, and spatial relationships.</p><span class="status">◌ PRIMITIVES + LESSON RECORD</span></article>
    <article class="tile"><span class="number">03 / THERMODYNAMICS</span><h3>Heat & energy</h3><p>Trace temperature and energy through bounded thermal models.</p><span class="status">◌ BOUNDED MODELS</span></article>
    <article class="tile"><span class="number">04 / FLUIDS</span><h3>Water & flow</h3><p>Explore shallow-water fields, rainfall accumulation, and conservation evidence.</p><span class="status">◌ BOUNDED MODELS</span></article>
    <article class="tile"><span class="number">05 / ACOUSTICS</span><h3>Waves & sound</h3><p>Compare direct and reflected paths in a declared room geometry.</p><span class="status">◌ ANALYTICAL MODELS</span></article>
    <article class="tile"><span class="number">06 / MATERIALS</span><h3>Structure & matter</h3><p>Discover the path from physical properties to stress, deformation, and failure.</p><span class="status">◇ RESEARCH DOMAIN</span></article>
  </div>
  <div class="panel"><div class="kicker">The learning loop</div><h2>Notice → Wonder → Predict → Observe → Explain</h2><p class="muted">A lesson should be approachable at first glance and inspectable all the way down: model identity, declared assumptions, source-backed samples, tests, and limitations.</p><div class="toolbar"><a class="button primary" href="#/experience/projectile" data-action="OPEN_PROJECTILE">Enter the first experiment →</a><a class="button" href="https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/PHYSICS_CAPABILITY_CATALOG.md">Capability catalog ↗</a></div></div>`;
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
function render(){
 document.querySelectorAll("nav [data-route]").forEach(a=>a.toggleAttribute("aria-current", (state.screen==="HOME"&&a.dataset.route==="home")||(state.screen!=="HOME"&&a.dataset.route==="experience")));
 if(state.screen==="HOME") home(); else if(state.screen==="PROJECTILE") projectile(); else inspector();
}
document.addEventListener("click",e=>{
 const action=e.target.closest("[data-action]")?.dataset.action;
 if(action==="OPEN_PROJECTILE"){e.preventDefault();send(action);}
 if(action==="BACK"){e.preventDefault();send(action);}
});
async function start(){
 try {
  const response=await fetch("./data/projectile-trajectory.run.json");
  if(!response.ok) throw new Error(`Run record request failed: ${response.status}`);
  record=await response.json();
  if(!record.modelId||!Array.isArray(record.samples)||record.samples.length!==record.definition.stepCount+1) throw new Error("The run record failed its structural checks.");
  render();
 } catch(error) {
  view.innerHTML=`<div class="panel"><h2>Physics data unavailable</h2><p class="muted">${String(error.message)}</p><p class="muted">The site will not invent replacement measurements. Check the published run-record artifact and try again.</p></div>`;
 }
}
start();
