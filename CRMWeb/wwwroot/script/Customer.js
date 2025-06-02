class Customer {
    //Customer List
    static init() {
        Customer.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListGroupId', data: response.data.CustomerGroup, value: 'Id', text: 'Description', subText: "SubText" });
            Dropdown.bind({ id: '#ListId', data: response.data.Customer, value: 'Id', text: 'Description', subText: "Code" });
        });
        $('#btnSearch').on('click', () => {
            Customer.get();
        });
        $('#btnPrint').on('click', () => {
            Customer.print();
        });
        $('#btnExport').on('click', () => {
            Customer.export();
        });
        $('#btnAdd').on('click', () => {
            Customer.fill();
        });
        Customer.initAdd();
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Customer/GetViewOption', onSuccess: onSuccess });
    }
    static get() {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListGroupId: $('#ListGroupId').val(),
            ListId: $('#ListId').val()
        }
        Data.post(
            {
                url: 'Customer/Get',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableCustomer', data: response.data });
                }
            }
        );
    }
    static print() {
        var obj = {
            ListRecordStatus: $('#ListRecordStatus').val(),
            ListGroupId: $('#ListGroupId').val(),
            ListId: $('#ListId').val()
        }
        Data.post(
            {
                url: 'Customer/Print',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableCustomer', data: response.data, isPrint: true });
                }
            }
        );
    }
    static export() {
        var obj = {
            ListRecordStatus: $('#ListRecordStatus').val(),
            ListGroupId: $('#ListGroupId').val(),
            ListId: $('#ListId').val()
        }
        Data.post(
            {
                url: 'Customer/Export',
                data: obj,
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fielName: "Customer" });
                }
            }
        );
    }
    //Customer Add
    static initAdd() {
        $('#Customer_PinCode').on('input', () => {
            let PinCode = $('#Customer_PinCode').val();
            OnlineApi.pinCode({
                pinCode: PinCode,
                postOfficeId: "#Customer_PostOffice",
                districtId: "#Customer_District",
                stateId: "#Customer_AdminDivId",
                countryId: "#Customer_CountryId"
            });
        });
        $('#Customer_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".Customer-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formCustomer" });
            if (!obj.Id) {
                Customer.add(obj);
            }
            else {
                Customer.update(obj);
            }
        });

    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'Customer/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        Customer.getAddOption((response) => {
            Modal.open({ id: '#modalCustomer', title: 'Customer / Add', action: 'Add' });
            $('#Customer_SeqNo').val(0);
            Dropdown.bind({ id: '#Customer_CountryId', data: response.data.Country, value: 'Id', text: 'Description', subText: 'SubText', addFn: 'CustomerGroup.fill', editFn: 'CustomerGroup.edit', deleteFn: 'CustomerGroup.delete' });
            Dropdown.bind({ id: '#Customer_AdminDivId', data: response.data.AdminDiv, value: 'Id', text: 'Description', subText: 'SubText', addFn: 'CustomerGroup.fill', editFn: 'CustomerGroup.edit', deleteFn: 'CustomerGroup.delete' });


        });
    }
    static add(obj) {
        Data.post(
            {
                url: 'Customer/Add',
                data: obj,
                onSuccess: Customer.addOnSuccess
            }
        );
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalCustomer" });
            Table.add({ id: "#tableCustomer", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.Customer, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit(id, viewMode = "Edit") {
        Data.get(
            {
                url: `Customer/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = viewMode == 'Edit' ? obj.Id : null;
                        let title = viewMode == 'Edit' ? `Customer / Edit (Code: ${obj.Code})` : `Customer / Add`;
                        Dropdown.bind({ id: '#Customer_CountryId', data: response.data.Country, value: 'Id', text: 'Description', subText: 'SubText' });
                        Dropdown.bind({ id: '#Customer_AdminDiv', data: response.data.AdminDiv, value: 'Value', text: 'Description' });
                        Modal.open({ id: '#modalCustomer', title: title, action: viewMode, obj: obj });
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
                url: 'Customer/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    Customer.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalCustomer" });
            Table.updateById({ id: "#tableCustomer", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Customer, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static delete(id) {
        Message.confirm(
            {
                msg: 'Do you want to delete??',
                confirmButtonText: 'Delete',
                denyButtonText: 'Dont Delete',
                data: id,
                onConfirm: (id) => {
                    Data.delete({ url: `Customer/Delete?Id=${id}`, onSuccess: Customer.deleteOnSuccess });
                }
            }
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableCustomer", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Customer, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static enable(id) {
        Message.confirm(
            {
                msg: 'Do you want to enable??',
                confirmButtonText: 'Enable',
                denyButtonText: 'Dont Enable',
                data: id,
                onConfirm: (id) => {
                    Data.update({ url: `Customer/Enable?Id=${id}`, onSuccess: Customer.enableOnSuccess });

                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableCustomer", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Customer, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
}
window.tableCustomerSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableCustomerAddress = (value, obj, index) => {
    let address = [];
    if (obj.Address1 != null && obj.Address1.trim() !== "") {
        address.push(obj.Address1.trim());
    }
    if (obj.Address2 != null && obj.Address2.trim() !== "") {
        address.push(obj.Address2.trim());
    }
    
    if (obj.PostOffice != null && obj.PostOffice.trim() !== "") {
        address.push(obj.PostOffice.trim());
    }
    if (obj.District != null && obj.District.trim() !== "") {
        address.push(obj.District.trim());
    }
    if (obj.CountryDesc != null && obj.CountryDesc.trim() !== "") {
        address.push(obj.CountryDesc.trim());
    }
    if (obj.AdminDivDesc != null && obj.AdminDivDesc.trim() !== "") {
        address.push(obj.AdminDivDesc.trim());
    }
    if (obj.PinCode != null && obj.PinCode.trim() !== "") {
        address.push(obj.PinCode.trim());
    }

    let fullAddress = address.join(', ');
    return fullAddress.match(/.{1,30}/g)?.join('<br>') || '';
};
window.tableCustomerName = (value, obj, index) => {
    return obj.Name.match(/.{1,30}/g).join('<br>');;
}
window.tableCustomerDescription = (value, obj, index) => {
    return obj.Description.match(/.{1,30}/g).join('<br>');;
}
tableCustomerStatus = (value, obj, index) => {
    return '<div class="' + obj.StatusCss + '">' + obj.StatusName + '<div>';
}
tableCustomerCreatedByAndAt = (value, obj, index) => {
    return obj.CreatedByName + "<br>" + moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss');
}
tableCustomerUpdatedByAndAt = (value, obj, index) => {
    return obj.UpdatedByName + "<br>" + moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss');
}
tableCustomerAction = (value, obj, index) => {
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
window.tableCustomerActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Customer.edit(obj.Id);
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Customer.edit(obj.Id, "Add");
    },
    'click .btn-delete': (e, value, obj, index) => {
        Customer.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        Customer.enable(obj.Id);
    }
}
