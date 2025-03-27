class Unit {
    static init() {
        //Unit View
        Unit.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.Unit, value: 'Id', text: 'Description', subText: 'Name' });
            Dropdown.bind({ id: '#ListUQCG', data: response.data.UQCG, value: 'Value', text: 'Description', subText: 'Value' });
        });
        $('#btnSearch').on('click', () => {
            Unit.get({
                ApiName: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableUnit', data: response.data })
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Unit.get({
                ApiName: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableUnit', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Unit.get({
                ApiName: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "Unit" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            Unit.fill();
        });
        //Unit Add, Update & Delete
        Unit.initAdd();        
    }
    //Unit View
    static getViewOption(onSuccess) {
        Data.get({ url: 'Unit/GetViewOption', onSuccess: onSuccess });
    }
    static get({ApiName, onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val(),
            ListUQCG: $('#ListUQCG').val(),
        };
        Data.post({ url: `Unit/${ApiName}`, data: obj, onSuccess: onSuccess });
    }
    //Unit Add, Update & Delete
    static initAdd() {        
        $('#Unit_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".unit-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formUnit" });
            if (Field.isNullOrEmpty(obj.Id)) {
                Unit.add(obj);
            }
            else {
                Unit.update(obj);
            }
        });
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'Unit/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        Unit.getAddOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#Unit_UQCG', data: response.data.UQCG, value: 'Value', text: 'Description', subText: 'Value' });
                Modal.open({ id: '#modalUnit', title: 'Unit / Add', action: 'add' });
            }
        });        
    }
    static add(obj) {
        Data.post({ url: 'Unit/Add', data: obj, onSuccess: Unit.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Unit.updateUiState({ action: 'Add', response: response });
        }
    }
    static edit(id, action ="Edit") {
        Data.get(
            {
                url: `Unit/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        Dropdown.bind({ id: '#Unit_UQCG', data: response.data.UQCG, value: 'Value', text: 'Description', subText: 'Value' });
                        let title = action == "Edit" ? `Unit / Edit (Code : ${response.obj.Code})` : `Unit / Add`;
                        response.obj.Id = action == "Edit" ? response.obj.Id : null;
                        Modal.open({ id: '#modalUnit', title: title, action: action, obj: response.obj });
                    }
                    else {
                        Message.alert(response);
                    }
                }
            }
        );
    }    
    static update(obj) {
        Data.update({ url: 'Unit/Update', data: obj, onSuccess: Unit.updateOnSuccess });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Unit.updateUiState({ response: response });
        }
    }
    static delete(id) {
        Message.confirm(
            {
                msg: "Do you want to delete???",
                confirmButtonText: "Delete",
                denyButtonText: "Don't Delete",
                data: id,
                onConfirm: (id) => {
                    Data.delete(
                        {
                            url: `Unit/Delete?Id=${id}`,
                            onSuccess: Unit.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Unit.updateUiState({ response: response });
        }
    }
    static enable(id) {
        Message.confirm(
            {
                msg: "Do you want to enable???",
                confirmButtonText: "Enable",
                denyButtonText: "Don't Enable",
                data: id,
                onConfirm: (id) => {
                    Data.update(
                        {
                            url: `Unit/Enable?Id=${id}`,
                            onSuccess: Unit.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Unit.updateUiState({ response: response });
        }
    }
    static updateUiState({ action = null, response }) {
        switch (action) {
            case "Add":
                Modal.reset({ id: '#modalUnit' });
                Table.add({ id: "#tableUnit", data: response.obj, action: 'prepend' });
                break;
            default:
                Modal.close({ id: '#modalUnit' });
                Table.updateById({ id: "#tableUnit", objId: response.obj.Id, obj: response.obj });
                break;
        }                        
        Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        Dropdown.bind({ id: '#ListId', data: response.data.Unit, value: 'Id', text: 'Description', subText: 'Name' });
        Dropdown.bind({ id: '#ListUQCG', data: response.data.UQCG, value: 'Value', text: 'Description', subText: 'Value' });        
    }
}
window.tableUnitSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableUnitStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableUnitCreatedBy = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableUnitUpdatedBy = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableUnitAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsEdit ?
                    `<li>
                        <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
                            <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
                        </a>
                    </li>` :``
                }
                ${obj.IsDuplicate ?
                    `<li>
                        <a href="#" class="dropdown-item text-primary-100 btn-duplicate" title="Duplicate">
                            <span class="fa fa-copy text-primary-100"></span>&nbsp;&nbsp;Duplicate
                        </a>
                    </li>` : ``
                }
                ${obj.IsDelete ?
                    `<li>
                        <a href="#" class="dropdown-item text-danger-100 btn-delete" title="Delete">
                            <span class="fa fa-trash text-danger-100"></span>&nbsp;&nbsp;Delete
                        </a>
                    </li>` : ``
                }
                ${obj.IsEnable ?
                    `<li>
                        <a href="#" class="dropdown-item text-success-100 btn-enable" title="Enable">
                            <span class="fa fa-toggle-on text-success-100"></span>&nbsp;&nbsp;Enable
                        </a>
                    </li>` : ``
                }
            </ul>
        </div>
    `;
    return actionBtn;
}
window.tableUnitActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Unit.edit(obj.Id);
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Unit.edit(obj.Id, "Add");
    },
    'click .btn-delete': (e, value, obj, index) => {
        Unit.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        Unit.enable(obj.Id);
    }
}