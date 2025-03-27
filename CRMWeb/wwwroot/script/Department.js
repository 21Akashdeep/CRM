class Department {
    static init() {
        Department.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.Department, value: 'Id', text: 'Name', subText: 'Description' });
        });
        $('#btnSearch').on('click', () => {
            Department.get({
                action: "Get",
                onSuccess: (response) => {
                    Table.add({ id: '#tableDepartment', data: response.data })
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Department.get({
                action: "Print",
                onSuccess: (response) => {
                    Table.add({ id: '#tableDepartment', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Department.get({
                action: "Export",
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "Department" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            Department.fill();
        });
        Department.initAdd();

    }
    //Department View
    static getViewOption(onSuccess) {
        Data.get({ url: 'Department/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val(),
            ListUQCG: $('#ListUQCG').val(),
        };
        Data.post({ url: `Department/${action}`, data: obj, onSuccess: onSuccess });
    }
    //Department Add, Edit, Update & Delete
    static initAdd() {
        $('#Department_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".item-group-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formDepartment" });
            if (Field.isNullOrEmpty(obj.Id)) {
                Department.add(obj);
            }
            else {
                Department.update(obj);
            }
        });
    }
    static fill() {
        Modal.open({ id: '#modalDepartment', title: 'Department / Add', action: 'add' });
    }
    static add(obj) {
        Data.post({ url: 'Department/Add', data: obj, onSuccess: Department.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Department.setUiState({ action: 'Add', response: response });
        }
    }
    static edit(id, action = "Edit") {
        Data.get(
            {
                url: `Department/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let title = action == "Edit" ? `Department / Edit (Code : ${response.obj.Code})` : `Department / Add`;
                        response.obj.Id = action == "Edit" ? response.obj.Id : null;
                        Modal.open({ id: '#modalDepartment', title: title, action: action, obj: response.obj });
                    }
                    else {
                        Message.alert(response);
                    }
                }
            }
        );
    }
    static update(obj) {
        Data.update({ url: 'Department/Update', data: obj, onSuccess: Department.updateOnSuccess });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Department.setUiState({ response: response });
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
                            url: `Department/Delete?Id=${id}`,
                            onSuccess: Department.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Department.setUiState({ response: response });
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
                            url: `Department/Enable?Id=${id}`,
                            onSuccess: Department.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Department.setUiState({ response: response });
        }
    }
    static setUiState({ action = null, response }) {
        switch (action) {
            case "Add":
                Modal.reset({ id: '#modalDepartment' });
                Table.add({ id: "#tableDepartment", data: response.obj, action: 'prepend' });
                break;
            default:
                Modal.close({ id: '#modalDepartment' });
                Table.updateById({ id: "#tableDepartment", objId: response.obj.Id, obj: response.obj });
                break;
        }
        Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        Dropdown.bind({ id: '#ListId', data: response.data.Unit, value: 'Id', text: 'Description', subText: 'Name' });
    }
}
window.tableDepartmentSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableDepartmentStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableDepartmentCreatedBy = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableDepartmentUpdatedBy = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableDepartmentAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsEdit ?
            `<li>
                        <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
                            <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
                        </a>
                    </li>` : ``
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
window.tableDepartmentActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Department.edit(obj.Id);
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Department.edit(obj.Id, "Add");
    },
    'click .btn-delete': (e, value, obj, index) => {
        Department.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        Department.enable(obj.Id);
    }
}