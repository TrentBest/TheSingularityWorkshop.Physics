import test from "node:test";
import assert from "node:assert/strict";
import {
  navigationTransitions,
  parseNavigationRoute,
  resolveNavigationTransition,
  routeForNavigationState
} from "../navigation-fsm.js";

test("deep links resolve to their declared screens", () => {
  assert.deepEqual(parseNavigationRoute("#/home"), { screen: "HOME", sample: 0 });
  assert.deepEqual(parseNavigationRoute("#/experience/projectile"), { screen: "PROJECTILE", sample: 0 });
  assert.deepEqual(parseNavigationRoute("#/experience/projectile/evidence"), { screen: "INSPECTOR", sample: 0 });
  assert.deepEqual(parseNavigationRoute("#/system/navigation"), { screen: "FSM", sample: 0, returnScreen: "HOME" });
  assert.equal(parseNavigationRoute("#/unknown").screen, "HOME");
});

test("each screen maps back to its canonical route", () => {
  assert.equal(routeForNavigationState({ screen: "HOME" }), "#/home");
  assert.equal(routeForNavigationState({ screen: "PROJECTILE" }), "#/experience/projectile");
  assert.equal(routeForNavigationState({ screen: "INSPECTOR" }), "#/experience/projectile/evidence");
  assert.equal(routeForNavigationState({ screen: "FSM" }), "#/system/navigation");
  assert.throws(() => routeForNavigationState({ screen: "MISSING" }), RangeError);
});

test("declared actions resolve to their expected destinations", () => {
  assert.equal(resolveNavigationTransition("HOME", "OPEN_PROJECTILE"), "PROJECTILE");
  assert.equal(resolveNavigationTransition("HOME", "INSPECT_FSM"), "FSM");
  assert.equal(resolveNavigationTransition("PROJECTILE", "INSPECT"), "INSPECTOR");
  assert.equal(resolveNavigationTransition("INSPECTOR", "BACK"), "PROJECTILE");
  assert.equal(resolveNavigationTransition("FSM", "BACK"), "RETURN");
});

test("undeclared actions are rejected instead of guessing a destination", () => {
  assert.equal(resolveNavigationTransition("HOME", "BACK"), null);
  assert.equal(resolveNavigationTransition("PROJECTILE", "LAUNCH_ROCKET"), null);
  assert.equal(resolveNavigationTransition("UNKNOWN", "BACK"), null);
});

test("transition table is immutable", () => {
  assert.equal(Object.isFrozen(navigationTransitions), true);
  for (const actions of Object.values(navigationTransitions)) assert.equal(Object.isFrozen(actions), true);
});
