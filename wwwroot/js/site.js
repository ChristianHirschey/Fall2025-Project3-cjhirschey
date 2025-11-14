(function () {
    $(function () {
        // initialize DataTables only for tables that have class 'datatable'.
        if (typeof $.fn.dataTable === "function") {
            $("table.table.datatable").each(function () {
                var $t = $(this);
                if (!$t.hasClass("dt-initialized")) {
                    $t.addClass("dt-initialized");

                    // if the table requests a simple display (no search/paging/info), use a simple datatable for styling
                    if ($t.hasClass('datatable-simple')) {
                        $t.DataTable({
                            responsive: true,
                            paging: false,
                            searching: false,
                            info: false,
                            ordering: false,
                            lengthChange: false
                        });
                    } else {
                        $t.DataTable({
                            responsive: true,
                            lengthMenu: [5, 10, 25, 50],
                            language: {
                                search: "_INPUT_",
                                searchPlaceholder: "Search..."
                            },
                            columnDefs: [{ orderable: false, targets: "no-sort" }]
                        });
                    }
                }
            });
        }

        // make clickable brand bounce
        $(".brand").on("click", function (e) {
            $(this).effect && $(this).effect("bounce", { times: 1 }, 300);
        });
    });
})();
