class Location {
    static init() {
        Location.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
        });
        $('#btnSearch').on('click', () => {
            Location.get({
                action: "Get",
                onSuccess: (response) => {
                    Table.add({ id: '#tableLocation', data: response.data })
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Location.get({
                action: "Print",
                onSuccess: (response) => {
                    Table.add({ id: '#tableLocation', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Location.get({
                action: "Export",
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "Location" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            Location.fill();
        });
        Location.initAdd();

    }
    //Location View
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Location/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post({ url: `Location/${action}`, data: obj, onSuccess: onSuccess });
    }
    //Location Add, Edit, Update & Delete
    static initAdd() {
        $('#Location_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".Location-required" })) {
                return; listid
            }
            let obj = Data.serializeToObject({ formId: "#formLocation" });
            if (!obj.Id) {
                Location.add(obj);
            }
            else {
                Location.update(obj);
            }
        });
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'Location/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        Modal.open({ id: '#modalLocation', title: 'Location / Add', action: 'add' });
        $('#Customer_SeqNo').val(0);
    }
    static add(obj) {
        console.log("akash")
        Data.post({ url: 'Location/Add', data: obj, onSuccess: Location.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalLocation" });
            Table.add({ id: "#tableLocation", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `Location/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `Location / Edit (Code: ${obj.Code})` : `Location / Add`;
                        Modal.open({ id: '#modalLocation', title: title, action: action, obj: obj });
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
                url: 'Location/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    Location.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalLocation" });
            Table.updateById({ id: "#tableLocation", objId: response.obj.Id, obj: response.obj });
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
                            url: `Location/Delete?Id=${id}`,
                            onSuccess: Location.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableLocation", objId: response.obj.Id, obj: response.obj });
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
                            url: `Location/Enable?Id=${id}`,
                            onSuccess: Location.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableLocation", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }

}
window.tableLocationSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableLocationStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusName}</div>`;
}
window.tableLocationCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableLocationUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableLocationAction = (value, obj, index) => {
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
window.tableLocationActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Location.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Location.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Location.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        Location.enable({ id: obj.Id });
    }
}