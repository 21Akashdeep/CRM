class Designation {
    static init() {
        Designation.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
        });
        $('#btnSearch').on('click', () => {
            Designation.get({
                action: "Get",
                onSuccess: (response) => {
                    Table.add({ id: '#tableDesignation', data: response.data })
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Designation.get({
                action: "Print",
                onSuccess: (response) => {
                    Table.add({ id: '#tableDesignation', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Designation.get({
                action: "Export",
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "Designation" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            Designation.fill();
        });
        Designation.initAdd();

    }
    //Designation View
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Designation/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post({ url: `Designation/${action}`, data: obj, onSuccess: onSuccess });
    }
    //Designation Add, Edit, Update & Delete
    static initAdd() {
        $('#Designation_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".designation-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formDesignation" });
            if (!obj.Id) {
                Designation.add(obj);
            }
            else {
                Designation.update(obj);
            }
        });
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'Designation/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        Modal.open({ id: '#modalDesignation', title: 'Designation / Add', action: 'add' });
        $('#Party_SeqNo').val(0);
    }
    static add(obj) {
        Data.post({ url: 'Designation/Add', data: obj, onSuccess: Designation.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalDesignation" });
            Table.add({ id: "#tableDesignation", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `Designation/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `Designation / Edit (Code: ${obj.Code})` : `Designation / Add`;
                        Modal.open({ id: '#modalDesignation', title: title, action: action, obj: obj });
                    }
                    else {
                        Message.show(response);
                    }
                }
            }
        );
    }
    static update(obj) {
        Data.update(
            {
                url: 'Designation/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    Designation.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalDesignation" });
            Table.updateById({ id: "#tableDesignation", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static delete({ id }) {
        Message.confirm(
            {
                msg: "Do you want to delete???",
                confirmButtonText: "Delete",
                denyButtonText: "Don't Delete",
                data: id,
                onConfirm: (id) => {
                    Data.delete(
                        {
                            url: `Designation/Delete?Id=${id}`,
                            onSuccess: Designation.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableDesignation", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }
    static enable({ id }) {
        Message.confirm(
            {
                msg: "Do you want to enable???",
                confirmButtonText: "Enable",
                denyButtonText: "Don't Enable",
                data: id,
                onConfirm: (id) => {
                    Data.update(
                        {
                            url: `Designation/Enable?Id=${id}`,
                            onSuccess: Designation.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableDesignation", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }

}
window.tableDesignationSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableDesignationStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusName}</div>`;
}
window.tableDesignationCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableDesignationUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableDesignationAction = (value, obj, index) => {
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
window.tableDesignationActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Designation.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Designation.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Designation.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        Designation.enable({ id: obj.Id });
    }
}