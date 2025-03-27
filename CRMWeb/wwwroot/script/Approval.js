class Approval
{
    static init() {
        Approval.getViewOption();
        $('#btnSearch').on('click', () => {
            Approval.get({
                ApiName: "Get",
                onSuccess: (response) => {
                    Table.add({ id: '#tableApproval', data: response.data });
                }
            });
        });
        $('#btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".approval-required" })) {
                return;
            }
            var obj = Data.serializeToObject({ formId: "#formApproval" });
            if ($('#btnSave').attr('data-action') == "Approve") {
                Approval.approve(obj);
            }
            else if ($('#btnSave').attr('data-action') == "Reject") {
                Approval.reject(obj);
            }
        });
    }
    static getViewOption() {
        Data.get({
            url: 'Approval/GetViewOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListApiId', data: response.data.Api, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#ListReqId', data: response.data.Req, value: 'ReqId', text: 'ReqNo' });
            }
        });
    }
    static get({ ApiName, onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListApiId: $('#ListApiId').val(),
            ListReqId: $('#ListReqId').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            TillDate: DateTime.json($('#DateRange').val().split('|')[1]),
        };
        Data.post({ url: `Approval/${ApiName}`, data: obj, onSuccess: onSuccess });
    }
    static getReqInfo({ obj, title, action = "View", onSuccess }) {
        let newObj = {
            ListReqId: [obj.ReqId],
            ApiName: obj.ApiName,
        };
        Data.post({
            url: `Approval/GetReqInfo`,
            data: newObj,
            onSuccess: (response) => {
                Modal.open({ id: "#modalApprovalReq", title: `Approval Req. ${obj.ApiName} / ${action}`, action: (action == "View" ? "View" : "Add"), obj: obj });
                let data = Approval.rebuildData({ apiName: obj.ApiName, data: response.data });
                Table.add({ id: '#tableApprovalReq', data: data, autoThead: true, removeCol: ["Item"], search: false });
                $('.detail-icon').trigger('click');
                $('#btnSave').attr('data-action', action);
                $('#btnSave').attr('class', `btn btn-sm ${action == "Approve" ? "btn-success" : "btn-danger"} rounded-5`);
                $('#btnSave').html('<span class="fa fa-check-double"></span>&nbsp;&nbsp;Approve');
                $('#btnSave').html(`<span class="fa fa-${action == "Approve" ? "check-double" : "ban"}"></span>&nbsp;&nbsp;${action}`);
            }
        });
    }
    static rebuildData({ apiName, data }) {
        let newData = [];
        if (apiName == App.Setting.ApiName.OutPass) {
            $(data).each((rowIndex, obj) => {
                let OutPass = {
                    OutPassNo: obj.Code,
                    Date: moment(obj.Date).format("DD-MMM-YYYY HH:mm:ss"),
                    NameOfPerson: obj.NameOfPerson,
                    Designation: obj.Designation,
                    FirmName: obj.FirmName,
                    VehicleNo: obj.VehicleNo,
                    DriverName: obj.DriverName,
                    TransferInfo: obj.TransferInfo,
                    IsReturnable: obj.IsReturnable,
                    Purpose: obj.Purpose,
                    Destination: obj.Destination,
                    Authority: obj.Authority,
                    Receipt: obj.Receipt,
                    CreatedByAndAt: `<div>${obj.CreatedByName}</div><div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`,                    
                    Item: []
                };
                $(obj.OutPassItem).each((rowIndex2, item) => {
                    let objItem = {
                        ItemDes: item.ItemDesc,
                        PrimaryUnit: item.PrimaryUnitDesc,
                        PrimaryQty: item.PrimaryQty,
                        SecondaryUnit: item.SecondaryUnitDesc,
                        SecondaryQty: item.SecondaryQty,
                        Remarks: item.Remarks
                    };
                    OutPass.Item.push(objItem);
                });
                newData.push(OutPass);
            });
        }
        return newData;
    }    
    static approve(obj) {
        Data.post({
            url: "Approval/Approve",
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalApprovalReq' });
                    Table.updateById({ id: "#tableApproval", objId: response.obj.Id, obj: response.obj });
                    Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                    Dropdown.bind({ id: '#ListApiId', data: response.data.Api, value: 'Id', text: 'Description' });
                    Dropdown.bind({ id: '#ListReqId', data: response.data.Req, value: 'ReqId', text: 'ReqNo' });
                }
            }
        });
    }
    static reject(obj) {
        Data.post({
            url: "Approval/Reject",
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalApprovalReq' });
                    Table.updateById({ id: "#tableApproval", objId: response.obj.Id, obj: response.obj });
                    Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                    Dropdown.bind({ id: '#ListApiId', data: response.data.Api, value: 'Id', text: 'Description' });
                    Dropdown.bind({ id: '#ListReqId', data: response.data.Req, value: 'ReqId', text: 'ReqNo' });
                }
            }
        });
    }
}
window.tableApprovalSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableApprovalDate = (value, obj, index) => {
    return moment(obj.ReqDate).format('DD-MMM-YYYY');
}
window.tableApprovalStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableApprovalAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end me-3">
                ${obj.IsView ?
                    `<li>
                        <a href="#" class="dropdown-item text-primary-100 btn-view" title="View">
                            <span class="fa fa-eye text-primary-100"></span>&nbsp;&nbsp;View
                        </a>
                    </li>` :``
                }             
                ${obj.IsApprove ?
                    `<li>
                        <a href="#" class="dropdown-item text-success-100 btn-approve" title="Approve">
                            <span class="fa fa-check-double text-success-100"></span>&nbsp;&nbsp;Approve
                        </a>
                    </li>` :``
                }                             
                ${obj.IsReject ?
                    `<li>
                        <a href="#" class="dropdown-item text-danger-100 btn-reject" title="Reject">
                            <span class="fa fa-ban text-danger-100"></span>&nbsp;&nbsp;Reject
                        </a>
                    </li>` : ``
                }                
            </ul>
        </div>
    `;
    return actionBtn;
}
window.tableApprovalEvent = {
    'click .btn-view': (e, value, obj, index) => {
        Approval.getReqInfo({ obj: obj, title: `Approval Req. ${obj.ApiName} / View`, action: "View" });
    },
    'click .btn-approve': (e, value, obj, index) => {
        Approval.getReqInfo({ obj: obj, title: `Approval Req. ${obj.ApiName} / Approve`, action: "Approve" });
    },
    'click .btn-reject': (e, value, obj, index) => {
        Approval.getReqInfo({ obj: obj, title: `Approval Req. ${obj.ApiName} / Reject`, action: "Reject" });
    },
}

//Table Approval req.
window.tableApprovalReqDetail = (value, obj, index) => {
    // Generate thead
    const thead = `
    <tr>${$.map(obj.Item[0], (value, fieldName) =>
        `<th>${fieldName.replaceAll('_', ' ').replace(/([A-Z])/g, ' $1').replace(/\s\s+/g, ' ').toCamelCase()}</th>`
    ).join('')}</tr>`;

    // Generate tbody
    const tbody = obj.Item.map(row => {
        return `<tr>
        ${$.map(row, (value, fieldName) =>
            `<td ${!isNaN(value) && value != null ? "class='td-num'" : ""}>${value == null ? "-" : value}</td>`
        ).join('')}</tr>`;
    }).join('');

    // Return the complete table
    return `
        <table class="table table-bordered table-default">
            <thead>${thead}</thead>
            <tbody>${tbody}</tbody>
        </table>
    `;
};

window.tableApprovalReqDate = (value, obj, index) => {
    return moment(obj.ReqDate).format('DD-MMM-YYYY');
}
window.tableApprovalReqStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div >`;
}