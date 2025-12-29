class GatePass {
    //GatePass List
    static init() {
        GatePass.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListLocation', data: response.data.Location, value: 'Id', text: 'Name' });
            Dropdown.bind({ id: '#ListDepartment', data: response.data.Department, value: 'Id', text: 'Department' });
            Dropdown.bind({ id: '#ListPassType', data: response.data.PassType, value: 'Value', text: 'Description' });
        });
        $('#btnSearch').on('click', () => {
            GatePass.get();
        });
        $('#btnPrint').on('click', () => {
            GatePass.print();
        });
        $('#btnExport').on('click', () => {
            GatePass.export();
        });
        $('#btnAdd').on('click', () => {
            GatePass.fill();
        });
        $('#btnNewEntry').on('click', () => {
            GatePass.newEntry();
        });
        GatePass.initAdd();
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'GatePass/GetViewOption', onSuccess: onSuccess });
    }
    static get() {
        var obj = {

            ListStatus: $('#ListStatus').val(),
            ListLocation: $('#ListLocation').val(),
            ListDepartment: $('#ListDepartment').val(),
            ListPassType: $('#ListPassType').val()
        }
        Data.post(
            {
                url: 'GatePass/Get',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableGatePass', data: response.data });
                }
            }
        );
    }
    static print() {
        var obj = {
            ListRecordStatus: $('#ListRecordStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post(
            {
                url: 'GatePass/Print',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableGatePass', data: response.data, isPrint: true });
                }
            }
        );
    }
    static export() {
        var obj = {
            ListRecordStatus: $('#ListRecordStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post(
            {
                url: 'GatePass/Export',
                data: obj,
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fielName: "GatePass" });
                }
            }
        );

    }

    //GatePass Add
    static initAdd() {
        $('#GatePass_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".GatePass-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formGatePass" });
            if (!obj.Id) {

                GatePass.add(obj);
            }
            else {
                GatePass.update(obj);
            }
        });
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'GatePass/GetAddOption', onSuccess: onSuccess });
    }
    static newEntry() {
        GatePass.getAddOption((response) => {
            console.log(response);
            Modal.open({
                id: '#modalGatePass',
                title: 'GatePass / Add',
                action: 'Add'
            });

            Dropdown.bind({
                id: '#GatePass_Employee',
                data: response.data.Employee,
                value: 'Id',
                text: 'Name'
            });
            Dropdown.bind({
                id: '#GatePass_PassType',
                data: response.data.PassType,
                value: 'Value',
                text: 'Description'
            });
            Dropdown.bind({
                id: '#GatePass_Location',
                data: response.data.Location,
                value: 'Id',
                text: 'Description'
            });
            Dropdown.bind({
                id: '#GatePass_Company',
                data: response.data.Company,
                value: 'Id',
                text: 'Description'
            });
        });
    }


    static add(obj) {
        Data.post(
            {
                url: 'GatePass/Add',
                data: obj,
                onSuccess: GatePass.addOnSuccess
            }
        );
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalGatePass" });
            Table.add({ id: "#tableGatePass", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.GatePass, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `GatePass/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `GatePass / Edit (Code: ${obj.Code})` : `GatePass / Add`;
                        Dropdown.bind({
                            id: '#GatePass_Employee',
                            data: response.data.Employee,
                            value: 'Id',
                            text: 'Name',
                            
                        });
                        Dropdown.bind({
                            id: '#GatePass_PassType',
                            data: response.data.PassType,
                            value: 'Value',
                            text: 'Description'
                        });
                        Dropdown.bind({
                            id: '#GatePass_Company',
                            data: response.data.Company,
                            value: 'Id',
                            text: 'Description',
                        });
                        Modal.open({ id: '#modalGatePass', title: title, action: action, obj: obj });

                    }
                    else {
                        Message.show(response);
                    }
                }
            }
        );
    }
    static update(obj) {
        Data.update({ url: 'GatePass/Update', data: obj, onSuccess: GatePass.updateOnSuccess });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalGatePass" });
            Table.updateById({ id: "#tableGatePass", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.GatePass, value: 'Id', text: 'Description', subText: "Code" });
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
                    Data.delete({ url: `GatePass/Delete?Id=${id}`, onSuccess: GatePass.deleteOnSuccess });
                }
            }
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableGatePass", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.GatePass, value: 'Id', text: 'Description', subText: "Code" });
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
                    Data.update({ url: `GatePass/Enable?Id=${id}`, onSuccess: GatePass.enableOnSuccess });

                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableGatePass", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.GatePass, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
}


window.tableGatePassName = (value, obj, index) => {
    return (obj.EmployeeDesc || "").match(/.{1,30}/g)?.join('<br>') || "-";
}

window.tableGatePassName = (value, obj, index) => {
    return (obj.LocationDesc || "").match(/.{1,30}/g)?.join('<br>') || "-";
}

window.tableGatePassIssueOn = (value, obj, index) => {
   return  `<div>${moment(obj.IssuedOn).format('DD-MMM-YYYY HH:mm:ss')}</div>`
}
window.tableGatePassExpiryOn = (value, obj, index) => {
    return `<div>${moment(obj.ExpiryOn).format('DD-MMM-YYYY HH:mm:ss')}</div>`
}
window.tableGatePassMedicalExpiryOn = (value, obj, index) => {
    return `<div>${moment(obj.MedicalExpiryOn).format('DD-MMM-YYYY HH:mm:ss')}</div>`
}
window.tableGatePassTrainingExpiryOn = (value, obj, index) => {
    return `<div>${moment(obj.TrainingExpiryOn).format('DD-MMM-YYYY HH:mm:ss')}</div>`
}
window.tableGatePassLabourLicenseExpiryOn = (value, obj, index) => {
    return `<div>${moment(obj.LabourLicenseExpiryOn).format('DD-MMM-YYYY HH:mm:ss')}</div>`
}
window.tableGatePassSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableGatePassName = (value, obj, index) => {
    return obj.Name.match(/.{1,30}/g).join('<br>');;
}
window.tableGatePassStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableGatePassCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableGatePassUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableGatePassAction = (value, obj, index) => {
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
window.tableGatePassActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        GatePass.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        GatePass.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        GatePass.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        GatePass.enable({ id: obj.Id });
    },
}