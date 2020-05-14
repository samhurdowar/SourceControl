

var dsPageTemplate = new kendo.data.DataSource({
    pageable: false,
    autoSync: false,
    transport: {
        read: {
            url: "@vd/PageTemplate/GetPageTemplate",
            dataType: "json"
        }
    },
    schema: {
        model: {
            id: "PageTemplateId",
            fields: {
                PageTemplateId: { editable: false, defaultValue: 0 }
            }
        }
    }
});


$("#gridPageTemplate").kendoGrid({
    dataSource: dsPageTemplate,
    autoBind: false,
    selectable: "single row",
    scrollable: false,
    pagable: false,
    height: (window.innerHeight - 212),
    columns: [
        { field: "TemplateName", title: "Templates" }
    ],
    editable: false,
    change: change_gridPageTemplate,
    dataBound: dataBound_gridPageTemplate
});

function change_gridPageTemplate(e) {
    try {

        var gridPageTemplate = $("#gridPageTemplate").data("kendoGrid").dataItem(this.select());
        PageTemplateId = gridPageTemplate.PageTemplateId;

        $("#tabPageTemplate").show();
        $("#divEditField").hide();

        $("#displayPageTemplateId").html(PageTemplateId);


        // ajax to get data
        $.ajax({
            url: "@vd/PageTemplate/GetPageTemplateTableForm",
            data: { pageTemplateId: PageTemplateId },
            dataType: "json",
            type: "POST",
            success: function (data) {

                try {
                    // disable all save buttons
                    DisableButton("cmdSavePageTemplateTable");
                    DisableButton("cmdSavePageTemplateColumn");
                    DisableButton("cmdSavePageTemplateLayout");

                    $('#GridColumns').data('kendoMultiSelect').destroy();
                    $('#GridColumnsDiv').empty();

                    $('#GridColumnsDiv').append('<input id="GridColumns" name="GridColumns" style="width:700px" />')

                } catch (e) {
                }

                try {
                    $('#ReportGridColumns').data('kendoMultiSelect').destroy();
                    $('#ReportGridColumnsDiv').empty();

                    $('#ReportGridColumnsDiv').append('<input id="ReportGridColumns" name="ReportGridColumns" style="width:700px" />')

                } catch (e) {
                }

                try {
                    $('#SortColumns').data('kendoMultiSelect').destroy();
                    $('#SortColumnsDiv').empty();

                    $('#SortColumnsDiv').append('<input id="SortColumns" name="SortColumns" style="width:700px" />')

                } catch (e) {
                }

                try {
                    $('#GroupByColumns').data('kendoMultiSelect').destroy();
                    $('#GroupByColumnsDiv').empty();

                    $('#GroupByColumnsDiv').append('<input id="GroupByColumns" name="GroupByColumns" style="width:700px" />')

                } catch (e) {
                }


                // Set DefaultSortColumn
                if (data.TableName.length > 2) {


                    $("#GridColumns").kendoMultiSelect({
                        placeholder: "Select field(s) to show in grid...",
                        dataValueField: "ValueField",
                        dataTextField: "TextField",
                        autoBind: false,
                        dataSource: {
                            transport: {
                                read: {
                                    dataType: "json",
                                    url: "@vd/PageTemplate/GetColumnOptions?pageTemplateId=" + data.PageTemplateId,
                                }
                            }
                        }
                    });

                    $("#ReportGridColumns").kendoMultiSelect({
                        placeholder: "Select field(s) to show in report...",
                        dataValueField: "ValueField",
                        dataTextField: "TextField",
                        autoBind: false,
                        dataSource: {
                            transport: {
                                read: {
                                    dataType: "json",
                                    url: "@vd/PageTemplate/GetColumnOptions?pageTemplateId=" + data.PageTemplateId,
                                }
                            }
                        }
                    });


                    $("#SortColumns").kendoMultiSelect({
                        placeholder: "Select field(s) to sort in grid...",
                        dataValueField: "ValueField",
                        dataTextField: "TextField",
                        autoBind: false,
                        dataSource: {
                            transport: {
                                read: {
                                    dataType: "json",
                                    url: "@vd/PageTemplate/GetSortColumnOptions?pageTemplateId=" + data.PageTemplateId,
                                }
                            }
                        }
                    });

                    $("#GroupByColumns").kendoMultiSelect({
                        placeholder: "Select field(s) to group in grid...",
                        dataValueField: "ValueField",
                        dataTextField: "TextField",
                        autoBind: false,
                        dataSource: {
                            transport: {
                                read: {
                                    dataType: "json",
                                    url: "@vd/PageTemplate/GetColumnOptions?pageTemplateId=" + data.PageTemplateId,
                                }
                            }
                        }
                    });


                }

                if (data.PageTemplateId2 > 0) {
                    var refKey2 = $("#RefKey2").data("kendoDropDownList");
                    refKey2.dataSource.transport.options.read.url = "@vd/PageTemplate/GetColumnOptions?pageTemplateId=" + data.PageTemplateId2;
                    refKey2.dataSource.read();

                    var primaryKey2 = $("#PrimaryKey2").data("kendoDropDownList");
                    primaryKey2.dataSource.transport.options.read.url = "@vd/PageTemplate/GetColumnOptions?pageTemplateId=" + data.PageTemplateId;
                    primaryKey2.dataSource.read();
                }


                // Set PageTemplate Form
                BindForm("FormPageTemplate", data);


                // Set ColumnDef
                SetkendoListView("ColumnDefs", "@vd/PageTemplate/GetColumnDefs?pageTemplateId=" + data.PageTemplateId);


                if (data.TableName.length < 2) {
                    $("#displayTableName_").html("<< Not mapped >>");
                    $(".tr-non-blank").hide();
                    $("#tabDetailFields").hide();
                    //$("#tabInlineFields").hide();
                } else {
                    $("#displayTableName_").html(data.TableName);
                    $(".tr-non-blank").show();
                    $("#tabDetailFields").show();
                    //$("#tabInlineFields").show();
                }

                // set Lookup tables for specific database
                SetkendoDropDownList("LookupTable", "@vd/PageTemplate/GetTableOptions?dbEntityId=" + $("#DbEntityId").data("kendoDropDownList").value());

                // set layout
                $("#PageTemplateLayout").data("kendoEditor").value(data.Layout);

            }
        });
    } catch (e) {
        alert(e);
    }

}


function dataBound_gridPageTemplate(e) {
    try {
        // select the current pageTemplateId
        var view = this.dataSource.view();
        for (var i = 0; i < view.length; i++) {
            if (view[i].id == PageTemplateId) {
                this.select(this.table.find("tr[data-uid='" + view[i].uid + "']"));
                break;
            }
        }
    } catch (e) {

    }

    try {
        $("#Table2").data().kendoGrid.destroy();
        $("#Table2").empty();
    } catch (e) {
    }

    $("#Table2").kendoDropDownList({
        optionLabel: {
            ValueField: "",
            TextField: ""
        },
        autoBind: false,
        dataValueField: "ValueField",
        dataTextField: "TextField",
        dataSource: {
            transport: {
                read: {
                    dataType: "json",
                    url: "@vd/PageTemplate/GetTableOptions?dbEntityId=" + $("#DbEntityId").data("kendoDropDownList").value()
                }
            }
        },
        change: function (e) {
            var dataItem = e.sender.dataItem();
            SetkendoDropDownList("RefKey2", "@vd/PageTemplate/GetColumnOptionsByName?tableName=" + dataItem.ValueField + "&dbEntityId=" + $("#DbEntityId").data("kendoDropDownList").value());
        }
    });

}

function DeletePageTemplate() {
    if (PageTemplateId == 0) {
        MessageBox("Information", "Select template to delete.", false);
        return;
    }

    ConfirmMessage("Warning", "Delete template?");

    $("#dialogYesButton").click(function () {
        try {

            $.ajax({
                url: "@vd/PageTemplate/DeletePageTemplate",
                data: { pageTemplateId: PageTemplateId },
                dataType: "text",
                type: "POST",
                success: function (response) {
                    if (response != "T") {
                        MessageBox("Information", response, false);
                    } else {
                        $("#gridPageTemplate").data("kendoGrid").dataSource.read();
                        PageTemplateId = 0;

                        $("#tabPageTemplate").hide();
                        $("#dialogYesNo").data("kendoWindow").close();
                    }
                },
                error: function (x, y, z) {
                    alert("Error DeletePageTemplate.");
                    $("#dialogYesNo").data("kendoWindow").close();
                }
            });

        } catch (e) {
            $("#dialogYesNo").data("kendoWindow").close();
        }

    })

    $("#dialogNoButton").click(function () {
        $("#dialogYesNo").data("kendoWindow").close();
    })
}


