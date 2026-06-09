(function () {
  const shell = document.querySelector("[data-admin-shell]");
  const toggle = document.querySelector("[data-admin-sidebar-toggle]");
  const key = "hooshyaran.admin.sidebar.collapsed";

  if (!shell || !toggle) {
    return;
  }

  const applyState = function (collapsed) {
    shell.classList.toggle("is-sidebar-collapsed", collapsed);
    toggle.setAttribute("aria-pressed", collapsed ? "true" : "false");
  };

  applyState(localStorage.getItem(key) === "true");

  toggle.addEventListener("click", function () {
    const collapsed = !shell.classList.contains("is-sidebar-collapsed");
    localStorage.setItem(key, collapsed ? "true" : "false");
    applyState(collapsed);
  });
})();
