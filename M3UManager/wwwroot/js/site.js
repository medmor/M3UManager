
function deselectGroupsItems() {
    document.querySelectorAll('.select-groups')
        .forEach((select) => {
            select.selectedIndex = -1;
        });
}