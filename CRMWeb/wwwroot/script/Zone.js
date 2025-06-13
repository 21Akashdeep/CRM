class Zone {
    //Zone List
    static init() {
        Zone.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.Zone, value: 'Id', text: 'Description', subText: "Code" });
        });
        $('#btnSearch').on('click', () => {
            Zone.get({
                action: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableZone', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Zone.get({
                action: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableZone', data: response.data, isPrint: true, reportDesc: 'Zone List' });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Zone.get({
                action: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "Zone" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            Zone.fill();
        });
        Zone.initAdd();
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Zone/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post({ url: `Zone/${action}`, data: obj, onSuccess: onSuccess });
    }

    //Zone Add
    static initAdd() {
        $('#Zone_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".zone-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formZone" });
            if (!obj.Id) {
                Zone.add(obj);
            }
            else {
                Zone.update(obj);
            }
        });

    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'Zone/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        Zone.getAddOption((response) => {
            Modal.open({ id: '#modalZone', title: 'Zone / Add', action: 'Add' });            
            Dropdown.bind({ id: '#Zone_Country', data: response.data.Country, value: 'Value', text: 'Description' });

        });
    }
    static add(obj) {
        Data.post(
            {
                url: 'Zone/Add',
                data: obj,
                onSuccess: Zone.addOnSuccess
            }
        );
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalZone" });
            Table.add({ id: "#tableZone", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.Zone, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `Zone/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `Zone / Edit (Code: ${obj.Code})` : `Zone / Add`;
                        Dropdown.bind({ id: '#Zone_Country', data: response.data.Country, value: 'Value', text: 'Description' });
                        Modal.open({ id: '#modalZone', title: title, action: action, obj: obj });                        
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
                url: 'Zone/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    Zone.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalZone" });
            Table.updateById({ id: "#tableZone", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Zone, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static delete({ id }) {
        Message.confirm(
            {
                msg: 'Do you want to delete??',
                confirmButtonText: 'Delete',
                denyButtonText: 'Dont Delete',
                data: id,
                onConfirm: (id) => {
                    Data.delete({ url: `Zone/Delete?Id=${id}`, onSuccess: Zone.deleteOnSuccess });
                }
            }
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableZone", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Zone, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        }
    }
    static enable({ id }) {
        Message.confirm(
            {
                msg: 'Do you want to enable??',
                confirmButtonText: 'Enable',
                denyButtonText: 'Dont Enable',
                data: id,
                onConfirm: (id) => {
                    Data.update({ url: `Zone/Enable?Id=${id}`, onSuccess: Zone.enableOnSuccess });

                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableZone", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Zone, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        }
    }
}
window.tableZoneSLNo = (value, obj, index) => {
    return index + 1;
}
tableZoneStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusName}<div>`;
}
tableZoneCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
tableZoneUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
tableZoneAction = (value, obj, index) => {
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
window.tableZoneActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Zone.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Zone.edit({ id: obj.Id, action: 'Add' })
    },
    'click .btn-delete': (e, value, obj, index) => {
        Zone.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        Zone.enable({ id: obj.Id });
    }
}
