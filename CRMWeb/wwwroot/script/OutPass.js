class OutPass {
    static Unit = [];
    static init() {
        OutPass.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.OutPass, value: 'Id', text: 'Code', subText: 'SubText' });
        });
        $('#btnSearch').on('click', () => {
            OutPass.get({
                ApiName: 'Get',
                onSuccess: (response) => {                    
                    Table.add({ id: '#tableOutPass', data: response.data });      
                }
            });
        });
        $('#btnPrint').on('click', () => {
            OutPass.get({
                ApiName: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: "#tableOutPass", data: response.data, isPrint: true, reportDesc: "Out Pass List" });
                }
            });
        });
        $('#btnExport').on('click', () => {
            OutPass.get({
                ApiName: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "OutPass" });                    
                }
            });
        });
        $('#btnAdd').on('click', () => {
            OutPass.fill();
        });
        //Add Out Pass
        OutPass.initAdd();
    }
    static getViewOption(onSuccess) {        
        Data.get({ url: `OutPass/GetViewOption`, onSuccess: onSuccess });
    }
    static get({ ApiName, onSuccess = () => { } }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            TillDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListId: $('#ListId').val(),
            ListIsOut: $('#ListIsOut').val().map(v => JSON.parse(v)),
            ListIsReturned: $('#ListIsReturned').val().map(v => JSON.parse(v))
        };
        Data.post({ url: `OutPass/${ApiName}`, data: obj, onSuccess: onSuccess });
    }
    static initAdd() {
        $('#btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.outpass-required' }))
                return;
            let OutPassItem = $('#tableOutPassItem').bootstrapTable('getData').filter(x => !Field.isNullOrEmpty(x.ItemDesc));
            if (OutPassItem.length == 0) {
                Message.error({ statusText: 'Out Pass Item is empty.' });
                return;
            }
            let obj = Data.serializeToObject({ formId: '#formOutPass' });
            obj.Date = DateTime.json(obj.Date);
            obj.IsReturnable = JSON.parse(obj.IsReturnable);
            obj.OutPassItem = OutPassItem;            
            if (Field.isNullOrEmpty(obj.Id)) {
                OutPass.add(obj);
            }
            else {
                OutPass.update(obj);
            }
        });
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'OutPass/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        OutPass.getAddOption((response) => {
            Modal.open({ id: '#modalOutPass', title: "Out Pass / Apply", action: 'Add' });
            OutPass.Unit = response.data.Unit;
            Dropdown.bind({ id: '#IsReturnable', data: response.data.Returnable, value: 'Value', text: 'Description' });
        });
        
    }
    static addItem() {
        let obj = {
            Id: 0,
            OutPassId: 0,
            ItemDesc: null,
            PrimaryUnitId: 0,
            PrimaryQty: 0,
            SecondaryUnitId: 1,
            SecondaryQty: 0,
            Remarks: null,
            IsDelete:false
        };
        Table.add({ id: '#tableOutPassItem', data: obj, action: 'append', selectPick: true, overflow: 'visible', height: 'auto', mobileResponsive: false });
    }
    static deleteItem(obj, index) {
        if (!obj.IsDelete) {
            Table.remove({ id: '#tableOutPassItem', value: [index] });
        }
        else {
            Message.confirm({
                msg: 'Do you want to delete',
                confirmButtonText: "Delete",
                denyButtonText: "Dont Delete",
                data: obj.Id,
                onConfirm: (Id) => {
                    Data.delete({
                        url: `OutPass/DeleteItem?Id=${Id}`,
                        onSuccess: (response) => {
                            Message.show(response);
                            if (response.status == Message.Type.success) {
                                Table.remove({ id: "#tableOutPassItem", value: [index] });
                            }
                        }
                    });
                }
            });
        }
    }
    static add(obj) {
        Data.post({ url: 'OutPass/Add', data: obj, onSuccess: OutPass.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status != Message.Type.error) {
            Modal.reset({ id: '#modalOutPass' });
            Table.add({ id: '#tableOutPass', data: response.obj, action: "prepend" });
        }
    }
    static edit(id, action = "Edit") {
        Data.get({
            url: `OutPass/Edit?Id=${id}`,
            onSuccess: (response) => {
                if (response.status == Message.Type.success) {
                    OutPass.Unit = response.data.Unit;
                    Dropdown.bind({ id: '#IsReturnable', data: response.data.Returnable, value: 'Value', text: 'Description' });
                    let obj = response.obj;
                    obj.Id = action == "Edit" ? obj.Id : null;
                    obj.Date = moment(obj.Date).format("DD-MMM-YYYY");
                    let title = action == "Edit" ? `Out Pass / Edit (Out Pass No. : ${obj.Code})` : `Out Pass / Add`;
                    Modal.open({ id: '#modalOutPass', title: title, action: action, obj: obj });
                    Table.add({ id: '#tableOutPassItem', data: obj.OutPassItem, selectPick: true, overflow: 'visible', height: 'auto', mobileResponsive:false });
                }
                else {
                    Message.error(response);
                }
            }
        });
    }
    static update(obj) {
        Data.update({ url: 'OutPass/Update', data: obj, onSuccess: OutPass.updateOnSuccess });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.close({ id: '#modalOutPass' });            
            Table.updateById({ id: "#tableOutPass", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.OutPass, value: 'Id', text: 'Code', subText: "Subtext" });
        }
    }
    static approvalLog(obj) {
        Data.post({
            url: `OutPass/ApprovalLog`,
            data: obj,
            onSuccess: (response) => {
                Modal.open({ id: '#modalOutPassApprovalLog', title: 'Outpass / Approval Log', action: 'View' });
                Table.add({ id: '#tableOutPassApprovalLog', data: response.data, search: false });
            }
        });
    }
    static delete(id) {
        Message.confirm({
            msg: 'Do you want to delete',
            confirmButtonText: "Delete",
            denyButtonText: "Dont Delete",
            data: id,
            onConfirm: (id) => {
                Data.delete({
                    url: `OutPass/Delete?Id=${id}`,
                    onSuccess: OutPass.deleteOnSuccess
                });
            }
        });        
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {            
            Table.updateById({ id: "#tableOutPass", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.OutPass, value: 'Id', text: 'Code', subText: "Subtext" });
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
                    Data.update({ url: `OutPass/Enable?Id=${id}`, onSuccess: OutPass.enableOnSuccess });

                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableOutPass", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.OutPass, value: 'Id', text: 'Code', subText: "SubText" });
        }
    }
    static printById(id) {
        Data.get({
            url: `OutPass/PrintById?Id=${id}`,
            onSuccess: (response) => {
                if (response.status == Message.Type.success) {
                    let obj = response.obj.OutPass;
                    let objFirstApprover = response.obj.FirstApprover;
                    let objLastApprover = response.obj.LastApprover;
                    let QRCodeBase64 = response.obj.QRCodeBase64;
                    let content = [];
                    let header = [];
                    let body = [];        
                    let footer = [];
                    header.push(`
                        <tr>
                            <th colspan="5" style="padding-left:130px; border-right:none;">
                                <div class="text-center brand-name">स्टील अथॉरिटी ऑफ इंडिया लिमिटेड</div>
                                <div class="text-center brand-name">STEEL AUTHORITY OF INDIA LIMITED</div>
                                <div class="text-center brand-title">रिसर्च एंड डेवलपमेंट सेंटर फॉर आयरन एंड स्टील</div>
                                <div class="text-center brand-title">R & D CENTER FOR IRON & STEEL</div>
                                <div class="text-center brand-title">आई.एस.ओ. - 9001 प्रमाणित</div>
                                <div class="text-center brand-title">I.S.O. - 9001 CERTIFIED</div>
                                <div class="text-center brand-title">दोरण्डा, रांची-2, DORANDA, RANCHI-2</div>
                                <div class="text-center brand-title">सामग्री हेतु वाह्य निर्गत विपत्र</div>
                                <div class="text-center brand-title">OUT GATE - PASS FOR MATERIALS</div>
                                <div class="text-center brand-title">(परिसर द्वार पर दिखाने हेतु, To be presented at Gate)</div>
                            </th>
                            <th class="v-align-top text-center" style="border-left:none;">
                                <img src="${QRCodeBase64}" style="height:100px;" />
                                <div class="text-right">Out Pass No.<br>${obj.Code}</div>
                            </th>
                        </tr>                            
                        <tr>
                            <th colspan="2" class="border-none-right border-none-bottom">
                                <div>विभाग / अनुविभाग</div>
                                <div>Section / Department</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-bottom"><b>:</b> ${obj.CreatedByDept}</td>
                        </tr>
                        <tr>
                            <th colspan="2" class="border-none-right border-none-top border-none-bottom">
                                <div>व्यक्ति का नाम जो सामग्री ले जा रहे हों</div>
                                <div>Name of person taking out material</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-top border-none-bottom"><b>:</b> ${obj.NameOfPerson} ${obj.Designation ?? ""}</td>
                        </tr>
                        <tr>
                            <th colspan="2" class="border-none-right border-none-top border-none-bottom">
                                <div>प्रतिनिधि संस्था का नाम / संविदक इत्यादि</div>
                                <div>Representing(Name of firm / Contractor, etc.)</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-top border-none-bottom"><b>:</b> ${obj.FirmName}</td>
                        </tr>
                        <tr>
                            <th colspan="2" class="border-none-right border-none-top border-none-bottom">
                                <div>वाहन संख्या</div>
                                <div>Vehicle No.</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-top border-none-bottom"><b>:</b> ${obj.VehicleNo ?? ""}</td>
                        </tr>
                        <tr>
                            <th colspan="2" class="border-none-right border-none-top border-none-bottom">
                                <div>चालक का नाम</div>
                                <div>Driver's Name</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-top border-none-bottom"><b>:</b> ${obj.DriverName ?? ""}</td>
                        </tr>
                        <tr>
                            <th colspan="2" class="border-none-right border-none-top border-none-bottom">
                                <div>बिका हुआ / अंक जो नीलाम हुआ / प्रदत</div>
                                <div>Sold / Auctioned as salvage / Issued</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-top border-none-bottom"><b>:</b> ${obj.TransferInfo ?? ""}</td>
                        </tr>
                        <tr>
                            <th colspan="2" class="border-none-right border-none-top border-none-bottom">
                                <div>वापस होने वाला / वापस नहीं होने वाला</div>
                                <div>Returnable / Non-Returnable</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-top border-none-bottom"><b>:</b>
                            ${obj.IsReturnable ? "Returnable" : "Non-Returnable"}</td>
                        </tr>
                        <tr>
                            <th colspan="2" class="border-none-right border-none-top border-none-bottom">
                                <div>उद्देश जिस हेतू बाहर ले जा रहे हो</div>
                                <div>Purpose for which taken out</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-top border-none-bottom"><b>:</b> ${obj.Purpose}</td>
                        </tr>
                        <tr>
                            <th colspan="2" class="border-none-right border-none-top border-none-bottom">
                                <div>गंतव्य स्थान</div>
                                <div>Destination</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-top border-none-bottom"><b>:</b> ${obj.Destination}</td>
                        </tr>
                        <tr>
                            <th colspan="2" class="border-none-right border-none-top border-none-bottom">
                                <div>प्राधिकारी</div>
                                <div>Authority</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-top border-none-bottom"><b>:</b> ${obj.Authority}</td>
                        </tr>
                        <tr>
                            <th colspan="2" class="border-none-right border-none-top">
                                <div>चलान / रोका प्राप्त आदि का संदर्भ</div>
                                <div>Refrance of entry Challan / Cash Memo</div>
                            </th>
                            <td colspan="4" class="border-none-left border-none-top"><b>:</b> ${obj.Receipt ?? ""}</td>
                        </tr>
                        <tr>
                            <th>SL No.</th>
                            <th>Name Of Materials</th>
                            <th>Qty.</th>
                            <th>Weight</th>
                            <th colspan="2">Remarks</th>
                        </tr>
                    `);
                    $(obj.OutPassItem).each((rowIndex, objItem) => {
                        body.push(`                                                    
                            <tr>
                                <td class="v-align-top">${(rowIndex + 1).toString().padStart(2, '0')}.</td>
                                <td class="v-align-top">${objItem.ItemDesc}</td>
                                <td class="v-align-top text-right">${objItem.PrimaryQty} ${objItem.PrimaryUnitDesc}</td>
                                <td class="v-align-top text-right">${objItem.SecondaryQty} ${objItem.SecondaryUnitDesc}</td>
                                <td class="v-align-top" colspan="2">${objItem.Remarks??""}</td>
                            </tr>                        
                        `);
                    });
                    footer.push(`
                        <tr>
                            <th colspan="2" class="border-none-right">
                                <div>प्राप्तकर्ता का हस्ताक्षर</div>
                                <div>Signature Of Receiver : _____________________________________</div>
                                <div>प्राप्ति की तिथि</div>
                                <div>Date Of Issue : <span class="fw-normal">${moment(obj.CreatedAt).format('DD-MMM-YYYY')}</span></div>
                            </th>
                            <th colspan="4" class="border-none-left v-align-top text-right">
                                <div>प्रदत्त पदाधिकारी का हस्ताक्षर एवं मोहर तथा पदनाम</div>
                                <div>Signature and Seal of Issuing Officer and Designation</div>
                                <div class="fw-normal text-underline">${objFirstApprover.UserName}</div>
                            </th>
                        </tr>       
                        <tr>
                            <th colspan="6" class="text-center">To Be Filled By Security</th>
                        </tr>
                        <tr>
                            <td colspan="6" class="p-0">
                                <table class="table-security">
                                    <colgroup>
                                        <col style="width:025px;" class="SLNo"/>
                                        <col style="width:170px;" class="Desc1"/>
                                        <col style="width:200px;" class="Value1"/>                                
                                        <col style="width:130px;" class="Desc2"/>
                                        <col style="width:200px;" class="Value2"/>                                        
                                    </colgroup>
                                    <tr>
                                        <th style="vertical-align: top!important;">(a)</th>
                                        <th>नाम<br>Name</th>
                                        <th colspan="3">: <span class="fw-normal">${objLastApprover.UserName}</span></th>                                        
                                    </tr>
                                    <tr>
                                        <th style="vertical-align: top!important;">(b)</th>
                                        <th>हस्ताक्षर<br>Signature</th>
                                        <th>: ______________________________</th>
                                        <th>दिनांक<br>Date</th>
                                        <th>: ______________________________</th>
                                    </tr>
                                    <tr>
                                        <th style="vertical-align: top!important;">(c)</th>
                                        <th>पंजीकृत<br>Registered No.</th>
                                        <th>: ______________________________</th>
                                        <th>क्रमांक<br>Sl. No.</th>
                                        <th>: ______________________________</th>
                                    </tr>
                                    <tr>
                                        <th style="vertical-align: top!important;">(d)</th>
                                        <th>निर्गमन तिथि और समय<br>Out Date & Time</th>
                                        <th>: <span class='fw-normal'>${obj.OutAt != null ? moment(obj.OutAt).format('DD-MMM-YYYY HH:mm:ss') : ''}</span></th>
                                        <th>आगमन तिथि और समय<br>In Date & Time</th>
                                        <th>: <span class='fw-normal'>${obj.ReceivedAt != null ? moment(obj.ReceivedAt).format('DD-MMM-YYYY HH:mm:ss') : ''}</span></th>
                                    </tr>
                                    <tr>
                                        <th style="vertical-align: top!important;">(e)</th>
                                        <th>नियंत्रित अधिकारी का हस्ताक्षर<br>Signature Of Controlling Officer</th>
                                        <th colspan="3">: ___________________________________</th>                                        
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    `);
                    content.push(`
                        <div class="page-container">
                            <div class="page">
                                <table>   
                                    <colgroup>
                                        <col style="width:030px;" class="SLNo" >
                                        <col style="width:330px;" class="NameOfMaterial" >
                                        <col style="width:075px;" class="Qty" >                                
                                        <col style="width:075px;" class="Weight" >
                                        <col style="width:130px;" class="Dummy" >
                                        <col style="width:110px;" class="Remarks" >                                
                                    </colgroup>
                                    ${header.join('')}
                                    ${body.join('')}
                                    ${footer.join('')}
                                </table>                                                                
                            </div>
                        </div>
                    `);
                    
                    let style = [];
                    style.push(`
                        table{width:100%; table-layout:fixed;}
                        table th{border: 1px solid black; text-align:left; padding-left:5px; padding-right: 5px;}
                        table td{border: 1px solid black; padding-left:5px; padding-right: 5px;}
                        td, th{font-size:12px;}
                        .table-security th,.table-security td{border: none!important; vertical-align: bottom;}
                    `);
                    style.push('table th,table td{border:1px solid black;}');
                    Print.page({ title: "Out Pass", style: style, content: content, isPrint: false });
                }
                else {
                    Message.show(response);
                }
            }
        });
    }
}
//Out Pass Table
window.tableOutPassSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableOutPassDate = (value, obj, index) => {
    return moment(obj.Date).format("DD-MMM-YYYY");
}
window.tableOutPassIsReturnable = (value, obj, index) => {
    return `<div class="${obj.IsReturnable ? "bg-primary" : "bg-success"} rounded-5 ps-3 pt-1 pb-1 pe-3 text-center">${obj.IsReturnable ? "Yes" : "No"}</div>`;
}
window.tableOutPassIsOut = (value, obj, index) => {
    let bgColor = obj.IsOut ? "bg-success" : "bg-danger";
    let text = obj.IsOut ? "Yes" : "No";
    return `<div class="${bgColor} rounded-5 ps-3 pt-1 pb-1 pe-3">${text}<div>`;
}
window.tableOutPassOutByAndAt = (value, obj, index) => {
    return `${obj.OutByName ?? "-"}<br>${obj.OutAt != null ? moment(obj.OutAt).format('DD-MMM-YYYY HH:mm:ss') : ''}`;
}
window.tableOutPassOutRemarks = (value, obj, index) => {
    return (obj.OutRemarks ?? "-").match(/.{1,40}/g).join('<br>');
}
window.tableOutPassIsReturned = (value, obj, index) => {
    let bgColor = obj.IsReturned == null || obj.IsReturned ? "bg-success" : "bg-danger";
    let text = obj.IsReturned == null ? "NA" : obj.IsReturned ? "Yes" : "No";
    return `<div class="${bgColor} rounded-5 ps-3 pt-1 pb-1 pe-3">${text}<div>`;
}
window.tableOutPassReceivedByAndAt = (value, obj, index) => {
    return `${obj.ReceivedByName ?? "-"}<br>${obj.ReceivedAt != null ? moment(obj.ReceivedAt).format('DD-MMM-YYYY HH:mm:ss') : ''}`;
}
window.tableOutPassReceivedRemarks = (value, obj, index) => {
    return (obj.ReceivedRemarks ?? "-").match(/.{1,40}/g).join('<br>');
}
window.tableOutPassStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss} text-nowrap">${obj.StatusDesc}<div>`;
}
window.tableOutPassCreatedByAndAt = (value, obj, index) => {
    return `${obj.CreatedByName}<br>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}`;
}
window.tableOutPassUpdatedByAndAt = (value,obj, index) => {
    return `${obj.UpdatedByName}<br>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}`;
}
window.tableOutPassAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end me-1">
                ${obj.IsEdit ?
                    `<li>
                        <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
                            <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
                        </a>
                    </li>` :``
                } 
                ${obj.IsPrint ?
                    `<li>
                        <a href="#" class="dropdown-item text-primary-100 btn-print" title="Print">
                            <span class="fa fa-print text-primary-100"></span>&nbsp;&nbsp;Print
                        </a>
                    </li>` :``
                } 
                ${obj.IsDuplicate ?
                    `<li>
                        <a href="#" class="dropdown-item text-primary-100 btn-duplicate" title="Duplicate">
                            <span class="fa fa-copy text-primary-100"></span>&nbsp;&nbsp;Duplicate
                        </a>
                    </li>` :``
                }             
                ${obj.IsApprovalLog ?
                    `<li>
                        <a href="#" class="dropdown-item text-primary-100 btn-approval-log" title="Approval Log">
                            <span class="fa fa-clock-rotate-left text-primary-100"></span>&nbsp;&nbsp;Approval Log
                        </a>
                    </li>` :``
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
window.tableOutPassActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        OutPass.edit(obj.Id);
    },
    'click .btn-print': (e, value, obj, index) => {
        OutPass.printById(obj.Id);
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        OutPass.edit(obj.Id, "Add");
    },
    'click .btn-approval-log': (e, value, obj, index) => {
        OutPass.approvalLog(obj);
    },
    'click .btn-delete': (e, value, obj, index) => {
        OutPass.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        OutPass.enable(obj.Id);
    }
}

//Out Pass Item
window.tableOutPassItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableOutPassItemDesc = (value, obj, index) => {
    return`        
        <input id="OutPassItemDesc_${index}" class="form-control form-control-sm outpass-item-desc outpass-required" value="${obj.ItemDesc == null ? '' : obj.ItemDesc}" placeholder="Name Of Material"/>
        <input id="OutPassItemRemarks_${index}" class="form-control form-control-sm outpass-item-remarks" value="${obj.Remarks == null ? '' : obj.Remarks}" placeholder="Remarks"/>
    `;
}
window.tableOutPassItemDescEvent = {
    'input .outpass-item-desc': (e, value, obj, index) => {
        obj.ItemDesc = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableOutPassItem', index: index, obj: obj, value: obj.ItemDesc, event: e });
    },
    'input .outpass-item-remarks': (e, value, obj, index) => {
        obj.Remarks = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableOutPassItem', index: index, obj: obj, value: obj.Remarks, event: e });
    },
}
window.tableOutPassItemPrimaryUnit = (value, obj, index) => {
    return Dropdown.html({ id: `OutPassItemPrimaryUnit_${index}`, class: 'form-control form-control-sm select-pick outpass-item-primary-unit outpass-required', data: OutPass.Unit, value: 'Id', text: 'Description', json: true, initialValue: [obj.PrimaryUnitId], title: 'Primary Unit', parent: '#modalOutPass', isSelectPick: true });
}
window.tableOutPassItemPrimaryUnitEvent = {
    'change .outpass-item-primary-unit': (e, value, obj, index) => {
        obj.PrimaryUnitId = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableOutPassItem', index: index, obj: obj, value: obj.PrimaryUnitId, event: e });
    }    
}
window.tableOutPassItemPrimaryQty = (value, obj, index) => {
    return `        
        <input id="OutPassItemSecondaryQty__${index}" class="form-control form-control-sm outpass-item-primary-qty outpass-required" value="${obj.PrimaryQty}" oninput="this.value = _Number.validate({value: this.value, dp:3})"/>
    `;
}
window.tableOutPassItemPrimaryQtyEvent = {
    'input .outpass-item-primary-qty': (e, value, obj, index) => {
        obj.PrimaryQty = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableOutPassItem', index: index, obj: obj, value: obj.PrimaryQty, event: e });
    }    
}
window.tableOutPassItemSecondaryUnit = (value, obj, index) => {
    return Dropdown.html({ id: `OutPassItemSecondaryUnit_${index}`, class: 'form-control form-control-sm select-pick outpass-item-secondary-unit', data: OutPass.Unit, value: 'Id', text: 'Description', json: true, initialValue: [obj.SecondaryUnitId], title: 'Secondary Unit', parent: '#modalOutPass', isSelectPick: true });
}
window.tableOutPassItemSecondaryUnitEvent = {
    'change .outpass-item-secondary-unit': (e, value, obj, index) => {
        obj.SecondaryUnitId = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableOutPassItem', index: index, obj: obj, value: obj.SecondaryUnitId, event: e });
    }
}
window.tableOutPassItemSecondaryQty = (value, obj, index) => {
    return `        
        <input id="OutPassItemSecondaryQty_${index}" class="form-control form-control-sm outpass-item-secondary-qty outpass-required"  value="${obj.SecondaryQty}" oninput="this.value = _Number.validate({value: this.value, dp:3})"/>
    `;
}
window.tableOutPassItemSecondaryQtyEvent = {
    'input .outpass-item-secondary-qty': (e, value, obj, index) => {
        obj.SecondaryQty = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableOutPassItem', index: index, obj: obj, value: obj.SecondaryQty, event: e });
    }
}
window.tableOutPassItemAction = (value, obj, index) => {    
    return `     
        <div class="btn-group">
            <button type="button" id="OutPassItemAction_${index}" class="btn btn-sm btn-danger btn-outpass-item-delete" 
                   style="padding-left:6px; padding-right:6px;" title="${obj.IsDelete ? "Delete Item" : "Remove Item"}">
                <span class="${obj.IsDelete ? "fa fa-trash" : "fa fa-times"}"></span>
            </button>
        </div>
    `;
}
window.tableOutPassItemActionEvent = {
    'click .btn-outpass-item-delete': (e, value, obj, index) => {
        OutPass.deleteItem(obj, index);
    }
}

//Table Out Pass Approval Log
window.tableOutPassApprovalLogSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableOutPassApprovalLogStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}<div>`;
}