class ApiGroup {
    static init() {
        ApiGroup.getViewOption((response) => {            
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.ApiGroup, value: 'Id', text: 'Description', subText: 'SubText' });
            Dropdown.bind({ id: '#ListParentId', data: response.data.ApiGroupParent, value: 'Id', text: 'Description', subText: 'SubText' });
        });        
        $('#btnSearch').on('click', () => {
            ApiGroup.get({
                Action: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableApiGroup', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            ApiGroup.get({
                Action: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableApiGroup', data: response.data, isPrint });
                }
            });
        });
        $('#btnExport').on('click', () => {
            ApiGroup.get({
                Action: 'Print',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "API Group" });
                }
            });
            
        });
        ApiGroup.initAdd();
        $('#btnAdd').on('click', () => {
            ApiGroup.fill();
        });
    }
    
    static getViewOption(onSuccess = () => { }) {
        Data.get(
            {
                url: 'ApiGroup/GetViewOption',
                loader: false,
                onSuccess: (response) => {
                    onSuccess(response);
                }
            }
        );
    }
    static get({ Action, onSuccess }) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val(),
            ListParentId: $('#ListParentId').val()
        }
        Data.post(
            {
                url: `ApiGroup/${Action}`,
                data: obj,
                onSuccess: onSuccess
            }
        );
    }

    static initAdd() {        
        $('#ApiGroup_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".api-group-required" })) {
                return;
            }
            var obj = Data.serializeToObject({ formId: "#formApiGroup" });
            obj.IsReserved = JSON.parse(obj.IsReserved);
            obj.ParentId = Field.isNullOrEmpty(obj.ParentId) ? 0 : obj.ParentId;
            obj.SeqNo = Field.isNullOrEmpty(obj.SeqNo) ? 0 : obj.SeqNo;
            if (Field.isNullOrEmpty(obj.Id)) {
                obj.Id = 0;
                ApiGroup.add(obj);
            }
            else {
                ApiGroup.update(obj);
            }
        });
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'ApiGroup/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        ApiGroup.getAddOption((response) => {
            Dropdown.bind({ id: '#ApiGroup_ParentId', data: response.data.ApiGroupParent, value: 'Id', text: 'Description' });
            Dropdown.bindIcon({ id: '#ApiGroup_Icon' });
            Modal.open({ id: '#modalApiGroup', title: 'Api Group / Add', action: 'Add' });
            SelectPick.set({ id: 'ApiGroup_ParentId', value: [0] });
            $('#ApiGroup_SeqNo').val('0');
        });        
    }
    static add(obj) {
        Data.post(
            {
                url: 'ApiGroup/Add',
                data: obj,
                onSuccess: (response) => {
                    ApiGroup.addOnSuccess(response);
                }
            }
        );
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: '#modalApiGroup' });
            Table.add({ id: "#tableApiGroup", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.ApiGroup, value: 'Id', text: ['Description'] });
            Dropdown.bind({ id: '#ListParentId', data: response.data.ApiGroupParent, value: 'Id', text: ['Description'] });
            Dropdown.bind({ id: '#ApiGroup_ParentId', data: response.data.ApiGroupParent, value: 'Id', text: ['Description'] });
        }
    };
    static edit({ id, action = 'Edit' }) {
        Data.get(
            {
                url: `ApiGroup/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `Api Group / Edit (Code : ${obj.Code})` : 'Api Group / Add';
                        obj.IsReserved = obj.IsReserved.toString();
                        Modal.open({ id: '#modalApiGroup', title: title, action: action, obj: obj });
                        Dropdown.bindIcon({ id: '#ApiGroup_Icon', value: [obj.Icon] });
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
                url: 'ApiGroup/Update',
                data: obj,
                onSuccess: (response) => {
                    ApiGroup.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.close({ id: '#modalApiGroup' });
            Table.updateById({ id: "#tableApiGroup", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ApiGroup, value: 'Id', text: ['Description'] });
            Dropdown.bind({ id: '#ListParentId', data: response.data.ApiGroupParent, value: 'Id', text: ['Description'] });
            Dropdown.bind({ id: '#ApiGroup_ParentId', data: response.data.ApiGroupParent, value: 'Id', text: ['Description'] });
        }
    };
    static delete(id) {
        Message.confirm(
            {
                msg: "Don you want to delete.",
                confirmButtonText: "Delete",
                denyButtonText: "Don't Delete",
                data: id,
                onConfirm: (Id) => {
                    Data.delete(
                        {
                            url: "ApiGroup/Delete?Id=" + Id,
                            onSuccess: (response) => {
                                ApiGroup.deleteOnSuccess(response);
                            }
                        }
                    );
                }
            });
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableApiGroup", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ApiGroup, value: 'Id', text: ['Description'] });
            Dropdown.bind({ id: '#ListParentId', data: response.data.ApiGroupParent, value: 'Id', text: ['Description'] });
            Dropdown.bind({ id: '#ApiGroup_ParentId', data: response.data.ApiGroupParent, value: 'Id', text: ['Description'] });
        }
    };
    static enable(id) {
        Message.confirm(
            {
                msg: "Don you want to enable.",
                confirmButtonText: "Enable",
                denyButtonText: "Don't Enable",
                data: id,
                onConfirm: (Id) => {
                    Data.update(
                        {
                            url: "ApiGroup/Enable?Id=" + Id,
                            onSuccess: (response) => {
                                ApiGroup.enableOnSuccess(response);
                            }
                        }
                    );
                }
            });
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableApiGroup", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ApiGroup, value: 'Id', text: ['Description'] });
            Dropdown.bind({ id: '#ListParentId', data: response.data.ApiGroupParent, value: 'Id', text: ['Description'] });
            Dropdown.bind({ id: '#ApiGroup_ParentId', data: response.data.ApiGroupParent, value: 'Id', text: ['Description'] });
        }
    };
}
window.tableApiGroupSlNoFormatter = (value, obj, index) => {
    return index + 1;
}
window.tableApiGroupIsReservedFormatter = (value, obj, index) => {
    return obj.IsReserved ? "Yes" : 'No';
}
window.tableApiGroupStatusFormatter = (value, obj, index) => {
    return '<div class="' + obj.StatusCss + '">' + obj.StatusName + '<div>';
}
window.tableApiGroupCreatedByAndAtFormatter = (value, obj, index) => {
    return obj.CreatedByName + "<br>" + moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss');
}
window.tableApiGroupUpdatedByAndAtFormatter = (value, obj, index) => {
    return obj.UpdatedByName + "<br>" + moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss');
}
window.tableApiGroupActionFormatter = (value, obj, index) => {
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
window.tableApiGroupActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        ApiGroup.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        ApiGroup.edit({ id: obj.Id, action: "Add" });
    },
    'click .btn-delete': (e, value, obj, index) => {
        ApiGroup.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        ApiGroup.enable(obj.Id);
    },
}