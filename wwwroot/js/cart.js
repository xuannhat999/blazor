document.addEventListener("change", function (event) {
  const target = event.target;
  if (!(target instanceof HTMLInputElement) || target.type !== "checkbox") return;

  const form = target.closest("form");
  if (!form) return;

  const selectAll = form.querySelector("[data-cart-select-all]");
  const boxes = Array.from(form.querySelectorAll('input[name="ids"][type="checkbox"]'));

  if (target === selectAll) {
    boxes.forEach(function (box) {
      box.checked = selectAll.checked;
    });
    selectAll.indeterminate = false;
    return;
  }

  if (!boxes.includes(target) || !selectAll) return;

  const checkedCount = boxes.filter(function (box) {
    return box.checked;
  }).length;

  selectAll.checked = boxes.length > 0 && checkedCount === boxes.length;
  selectAll.indeterminate = checkedCount > 0 && checkedCount < boxes.length;
});
