export const navigationTransitions = Object.freeze({
  HOME: Object.freeze({ OPEN_PROJECTILE: "PROJECTILE", INSPECT_FSM: "FSM" }),
  PROJECTILE: Object.freeze({ BACK: "HOME", INSPECT: "INSPECTOR", SELECT_SAMPLE: "PROJECTILE", INSPECT_FSM: "FSM" }),
  INSPECTOR: Object.freeze({ BACK: "PROJECTILE", SELECT_SAMPLE: "INSPECTOR", INSPECT_FSM: "FSM" }),
  FSM: Object.freeze({ BACK: "RETURN" })
});

export function resolveNavigationTransition(screen, action) {
  return navigationTransitions[screen]?.[action] ?? null;
}

export function routeForNavigationState(state) {
  switch (state.screen) {
    case "HOME": return "#/home";
    case "PROJECTILE": return "#/experience/projectile";
    case "INSPECTOR": return "#/experience/projectile/evidence";
    case "FSM": return "#/system/navigation";
    default: throw new RangeError(`Unknown Physics Pages screen: ${state.screen}`);
  }
}

export function parseNavigationRoute(hash) {
  if (hash === "#/system/navigation") return { screen: "FSM", sample: 0, returnScreen: "HOME" };
  if (hash === "#/experience/projectile/evidence") return { screen: "INSPECTOR", sample: 0 };
  if (hash === "#/experience/projectile") return { screen: "PROJECTILE", sample: 0 };
  return { screen: "HOME", sample: 0 };
}
