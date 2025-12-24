class Employee {
    //Employee List
    static init() {
        Employee.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListQualification', data: response.data.Qualification, value: 'Id', text: 'Description' });
            Dropdown.bind({ id: '#ListJntvtiCategory', data: response.data.JntvtiCategory, value: 'value', text: 'Description'});
        });
        $('#btnSearch').on('click', () => {
            Employee.get();
        });
        $('#btnPrint').on('click', () => {
            Employee.print();
        });
        $('#btnExport').on('click', () => {
            Employee.export();
        });
        $('#btnAdd').on('click', () => {
            Employee.fill();
        });
        $('#btnNewEntry').on('click', () => {
            Employee.newEntry();
        });
        Employee.initAdd();
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Employee/GetViewOption', onSuccess: onSuccess });
    }
    static get() {
        var obj = {

            ListStatus: $('#ListStatus').val(),
            ListGender: $('#ListGender').val(),
            ListJntvtiCategory: $('#ListJntvtiCategory').val(),
            ListQualification: $('#ListQualification').val()
        }
        Data.post(
            {
                url: 'Employee/Get',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableEmployee', data: response.data });
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
                url: 'Employee/Print',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableEmployee', data: response.data, isPrint: true });
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
                url: 'Employee/Export',
                data: obj,
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fielName: "Employee" });
                }
            }
        );

    }

    //Employee Add
    static initAdd() {
        $('#Employee_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".Employee-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formEmployee" });
            if (!obj.Id) {

                Employee.add(obj);
            }
            else {
                Employee.update(obj);
            }
        });
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'Employee/GetAddOption', onSuccess: onSuccess });
    }
    static newEntry() {
        Employee.getAddOption((response) => {
            console.log(response);
            Modal.open({
                id: '#modalEmployee',
                title: 'Employee / Add',
                action: 'Add'
            });

            Dropdown.bind({
                id: '#Employee_Qualification',
                data: response.data.qualification,
                value: 'Id',
                text: 'Description'
            });
            Dropdown.bind({
                id: '#Employee_JntvtiCategory',
                data: response.data.JntvtiCategory,
                value: 'Id',
                text: 'Description'
            });
        });
    }


    static add(obj) {
        Data.post(
            {
                url: 'Employee/Add',
                data: obj,
                onSuccess: Employee.addOnSuccess
            }
        );
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalEmployee" });
            Table.add({ id: "#tableEmployee", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.Employee, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `Employee/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `Employee / Edit (Code: ${obj.Code})` : `Employee / Add`;
                        Dropdown.bind({
                            id: '#Employee_Qualification',
                            data: response.data.qualification,
                            value: 'Id',
                            text: 'Description'
                        });
                        Dropdown.bind({
                            id: '#Employee_JntvtiCategory',
                            data: response.data.JntvtiCategory,
                            value: 'Id',
                            text: 'Description'
                        });
                        Modal.open({ id: '#modalEmployee', title: title, action: action, obj: obj });

                    }
                    else {
                        Message.show(response);
                    }
                }
            }
        );
    }
    static update(obj) {
        Data.update({ url: 'Employee/Update', data: obj, onSuccess: Employee.updateOnSuccess });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalEmployee" });
            Table.updateById({ id: "#tableEmployee", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Employee, value: 'Id', text: 'Description', subText: "Code" });
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
                    Data.delete({ url: `Employee/Delete?Id=${id}`, onSuccess: Employee.deleteOnSuccess });
                }
            }
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableEmployee", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Employee, value: 'Id', text: 'Description', subText: "Code" });
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
                    Data.update({ url: `Employee/Enable?Id=${id}`, onSuccess: Employee.enableOnSuccess });

                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableEmployee", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Employee, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
}

window.tableEmployeeGender = (value, obj, index) => {
    switch (obj.Gender) {
        case 'M': return 'Male';
        case 'F': return 'Female';
        case 'T': return 'Transgender';
        default: return '';
    }
};

window.tableEmployeeDob = (value, obj, index) => {
    if (!obj.DOB) return '-';

    return moment(obj.DOB).format('DD-MMM-YYYY');
};
window.tableEmployeeSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableEmployeeName = (value, obj, index) => {
    return obj.Name.match(/.{1,30}/g).join('<br>');;
}
window.tableEmployeeStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableEmployeeCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableEmployeeUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableEmployeeAction = (value, obj, index) => {
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
window.tableEmployeeActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Employee.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Employee.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Employee.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        Employee.enable({ id: obj.Id });
    },
}