class Api {
    //Api List
    static init() {        
        Api.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListGroupId', data: response.data.ApiGroup, value: 'Id', text: 'Description', subText: "SubText" });
            Dropdown.bind({ id: '#ListId', data: response.data.Api, value: 'Id', text: 'Description', subText: "Code" });            
        });
        $('#btnSearch').on('click', () => {
            Api.get();
        });
        $('#btnPrint').on('click', () => {
            Api.print();
        });
        $('#btnExport').on('click', () => {
            Api.export();
        });
        $('#btnAdd').on('click', () => {
            Api.fill();
        });        
        Api.initAdd();
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Api/GetViewOption', onSuccess: onSuccess });
    }
    static get() {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListGroupId: $('#ListGroupId').val(),
            ListId: $('#ListId').val()
        }
        Data.post(
            {
                url: 'Api/Get',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableApi', data: response.data });
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
                url: 'Api/Print',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableApi', data: response.data, isPrint: true });
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
                url: 'Api/Export',
                data: obj,
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fielName: "Api" });
                }
            }
        );
    }
    //Api Add
    static initAdd() {        
        $('#Api_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".api-required" })) {
                return;
            }
            var obj = Data.serializeToObject({ formId: "#formApi" });
            obj.IsApprovalRequired = JSON.parse(obj.IsApprovalRequired);
            obj.SeqNo = Field.isNullOrEmpty(obj.SeqNo) ? 0 : obj.SeqNo;
            if (!obj.Id) {
                obj.Id = 0;
                Api.add(obj);
            }
            else {
                Api.update(obj);
            }
        });
        ApiGroup.initAdd();
        ApiGroup.addOnSuccess = (response) => {
            Message.show(response);
            if (response.status == Message.Type.success) {
                Modal.close({ id: '#modalApiGroup' });
                let obj = response.obj;
                Dropdown.bind({ id: '#Api_ApiGroupId', data: response.data.ApiGroup, value: 'Id', text: 'Description', subText: 'SubText', initialValue: [obj.Id] });
            }
        }
        ApiGroup.updateOnSuccess = (response) => {
            Message.show(response);
            if (response.status == Message.Type.success) {
                Modal.close({ id: '#modalApiGroup' });
                let obj = response.obj;
                Dropdown.bind({ id: '#Api_ApiGroupId', data: response.data.ApiGroup, value: 'Id', text: 'Description', subText: 'SubText', initialValue: [obj.Id] });
            }
        }
        ApiGroup.deleteOnSuccess = (response) => {
            Message.show(response);
            if (response.status == Message.Type.success) {
                Modal.close({ id: '#modalApiGroup' });
                let obj = response.obj;
                Dropdown.bind({ id: '#Api_ApiGroupId', data: response.data.ApiGroup, value: 'Id', text: 'Description', subText: 'Code' });
            }
        }
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'Api/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        Api.getAddOption((response) => {
            Modal.open({ id: '#modalApi', title: 'API / Add', action: 'Add' });
            $('#Api_SeqNo').val(0);
            Dropdown.bind({ id: '#Api_ApiGroupId', data: response.data.ApiGroup, value: 'Id', text: 'Description', subText: 'SubText', addFn: 'ApiGroup.fill', editFn: 'ApiGroup.edit', deleteFn: 'ApiGroup.delete' });
            Dropdown.bind({ id: '#Api_ApiType', data: response.data.ApiType, value: 'Value', text: 'Description' });
            Dropdown.bindIcon({ id: '#Api_Icon' });            
        });        
    }
    static add(obj) {
        Data.post(
            {
                url: 'Api/Add',
                data: obj,
                onSuccess: Api.addOnSuccess
            }
        );
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalApi" });
            Table.add({ id: "#tableApi", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.Api, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit(id, viewMode = "Edit") {
        Data.get(
            {
                url: `Api/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = viewMode == 'Edit' ? obj.Id : null;
                        let title = viewMode == 'Edit' ? `Api / Edit (Code: ${obj.Code})` : `Api / Add`;
                        Modal.open({ id: '#modalApi', title: title, action: viewMode, obj: obj });

                        Dropdown.bind({ id: '#Api_ApiGroupId', data: response.data.ApiGroup, value: 'Id', text: 'Description', subText: 'SubText', addFn: 'ApiGroup.fill', editFn: 'ApiGroup.edit', deleteFn: 'ApiGroup.delete', initialValue: [obj.ApiGroupId] });
                        Dropdown.bind({ id: '#Api_ApiType', data: response.data.ApiType, value: 'Value', text: 'Description', initialValue: [obj.ApiType] });
                        Dropdown.bindIcon({ id: '#Api_Icon', value: [obj.Icon] });                                                
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
                url: 'Api/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    Api.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalApi" });
            Table.updateById({ id: "#tableApi", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Api, value: 'Id', text: 'Description', subText: "Code" });
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
                    Data.delete({ url: `Api/Delete?Id=${id}`, onSuccess: Api.deleteOnSuccess });
                }
            }
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableApi", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Api, value: 'Id', text: 'Description', subText: "Code" });
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
                    Data.update({ url: `Api/Enable?Id=${id}`, onSuccess: Api.enableOnSuccess });
                    
                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableApi", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Api, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
}
tableApiIsApprovalRequired = (value, obj, index) => {
    return obj.IsApprovalRequired ? "Yes" : "No";
}
tableApiStatusFormatter = (value, obj, index) => {
    return '<div class="' + obj.StatusCss + '">' + obj.StatusName + '<div>';
}
tableApiCreatedByAndAtFormatter = (value, obj, index) => {
    return obj.CreatedByName + "<br>" + moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss');
}
tableApiUpdatedByAndAtFormatter = (value,obj, index) => {
    return obj.UpdatedByName + "<br>" + moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss');
}
tableApiActionFormatter = (value, obj, index) => {
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
window.tableApiActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Api.edit(obj.Id);
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Api.edit(obj.Id, "Add");
    },
    'click .btn-delete': (e, value, obj, index) => {
        Api.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        Api.enable(obj.Id);
    },
}