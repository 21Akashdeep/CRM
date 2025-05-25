
document.addEventListener('DOMContentLoaded', () => {    
    Layout.setPermission();
    Layout.setPage();
    Layout.setMenu();
    Layout.init();
});
class Layout {
    static init() {    
        //View Password
        Util.init();
        //Default Table init
        Table.empty({ selector: '.table-default' });
        //Date Time init
        DateTime.init();
        //Side Bar On or Off Button
        $('.sidebar-toggle').on('click', () => {
            $('.sidebar .menu .child').css({ 'display': 'none' });
            if ($('.sidebar').width() >= 249 && $('.sidebar').width() <= 250) {
                Layout.sidebarOff();
            } else {
                Layout.sidebarOn();
            }
        });
        //On Window Resize Sidebar will be off or on
        window.addEventListener('resize', (event) => {            
            if ($(window).width() <= 1000) {
                Layout.sidebarOff();
            }
            else {
                if (Cookie.get('SidebarView') != "Off") {
                    Layout.sidebarOn();
                }
            }
        }, true);
        //On Hover Side bar will be show or Hide
        $('.sidebar').on('mouseenter mouseleave', (e) => {
            switch (e.type) {
                case 'mouseenter':
                    if ($(window).width() > 1000 && ($('.sidebar').width() >= 48 && $('.sidebar').width() <= 50)) {
                        $('.sidebar').css({ 'width': '250px', 'z-index': '2000', 'opacity': '0.98' });
                        let windowSize = $(window).width() <= 1000 ? '100%' : ($(window).width() - 50) + 'px';
                        let left = $(window).width() <= 1000 ? '0px' : '50px';
                        $('.content').css({ 'width': windowSize, left: left, top: '0', 'position': 'absolute' });
                        $('.app-name').text(App.Info.Name);
                    }
                    break;
                case 'mouseleave':
                    if ($(window).width() > 1000 && $('.sidebar').css('z-index') == '2000') {
                        $('.sidebar').css({ 'width': '50px', 'z-index': '', 'opacity': '1' });
                        $('.sidebar .menu .child').css({ 'display': 'none' });
                        $('.content').css({ 'width': '', 'position': '' });                        
                        $('a.menu-item').find('.fa.fa-angle-right').css({ "transform": "rotate(0deg)" });
                        $('.app-name').text(App.Info.Code);
                    }
                    break;
            }
        });
        //On Document Click Side bar Hide
        $(document).on('click', (e) => {            
            let sideNavigation = $('.sidebar');
            let sideNavToggle = $('.sidebar-toggle');
            if (!sideNavToggle.is(e.target) && sideNavToggle.has(e.target).length == 0 && !sideNavigation.is(e.target) && sideNavigation.has(e.target).length == 0 && $(window).width() <= 1000) {
                Layout.sidebarOff();
            }            
        });                
        //Modal Minimize
        let modalData;
        $("button[data-bs-minimize='toggle']").on("click", (e) => {
            let modalId = $(e.currentTarget).closest(".modal").attr("id");
            modalData = $(e.currentTarget).closest(".modal");
            let modal = "#" + modalId;
            $(".modal-backdrop").addClass("hide");
            $(modal).toggleClass("modal-minimize");
            if ($(modal).hasClass("modal-minimize")) {
                $(".modal-minimize-maximize").append(modalData);
                $(e.currentTarget).find("span").toggleClass('fa-minus').toggleClass('fa-clone');
            } else {
                $(".container").append(modalData);
                $(e.currentTarget).find("span").toggleClass('fa-clone').toggleClass('fa-minus');
            };
        });
        $("button[data-bs-dismiss='modal']").on('click', (e) => {
            $(e.currentTarget).closest(".modal").removeClass("modal-minimize");
            $(".container").removeClass(modalData);
            $(e.currentTarget).prev("button[data-bs-minimize='toggle']").find("span").removeClass('fa fa-clone').addClass('fa fa-minus');
        });
        //Modal Dragable
        $('.modal-draggable').draggable({
            handle: ".modal-header, .modal-body, .modal-footer"
        });
        //Due to modal focus problem when close the modal
        Modal.hideEvent();
        //input Text Upper Case
        $("input.text-upper").on('input', (e) => {
            $(e.currentTarget).val($(e.currentTarget).val().toUpperCase());
        });
        //Edit Profile
        $('#btnEditProfile').on('click', () => {
            Modal.open({ id: '#modalEditProfile', title: 'Theme / Set', action: 'Edit' });
        });
        //Update Profile
        $('#btnUpdateProfile').on('click', () => {
            if (!Field.isMandatory({ class: '.profile-required' })) {
                return;
            }
            let obj = {
                Theme: $('#Layout_Theme').val(),
                ApiType: App.Setting.ApiType.WebApp
            };
            if (!obj.Id) {
                Data.update({
                    url: 'User/UpdateProfile',
                    data: obj,
                    onSuccess: (response) => {
                        Message.show(response);
                        if (response.status == Message.Type.success) {
                            Modal.close({ id: '#modalEditProfile' });
                            let obj = response.data;
                            SessionStorage.set('User', JSON.stringify(obj.User));
                            SessionStorage.set('Permission', JSON.stringify(obj.Permission));
                            SessionStorage.set('AppMenu', Data.menuHtmlString(obj.AppMenu));                            
                            SessionStorage.set('Company', JSON.stringify(obj.Company));
                            location.reload();
                        }
                    }
                });
            }
        });
        //Edit Password
        $('#btnEditPassword').on('click', () => {
            Modal.open({ id: '#modalEditPassword', title: 'Change Password', action: 'Edit' });
        });
        //Change Password
        $('#btnUpdatePassword').on('click', () => {
            if (!Field.isMandatory({ class: '.update-password-required' })) {
                return;
            }
            let obj = {
                Password: $('#Password').val(),
                NewPassword: $('#NewPassword').val(),
                ConfirmPassword: $('#ConfirmPassword').val()
            };
            Data.update(
                {
                    url: 'User/UpdatePassword',
                    data: obj,
                    onSuccess: (response) => {
                        Message.show(response);
                        if (response.status == Message.Type.success) {
                            $('#modalEditPassword').modal('hide');
                        }
                    }
                }
            );
        });            
    }
    static sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    static async runProgressBar() {
        let progressBarValueNow = parseInt($('#ProgressBar').attr('aria-valuenow'));
        let progressBarValueMax = parseInt($('#ProgressBar').attr('aria-valuemax'));
        for (let loop = progressBarValueNow; loop <= progressBarValueMax; loop++) {
            if (document.readyState == "complete") {
                loop = progressBarValueMax;
            }
            $('#ProgressBar').attr('style', "width: " + loop + "%;");
            $('#ProgressBar').attr('aria-valuenow', loop);

            await Layout.sleep(1000);
            if (loop == progressBarValueMax) {
                $('.progress').addClass('hide');
            }
        }
    }
    static sidebarOn(speed = 500) {
        if ($(window).width() <= 1000) {
            $('.sidebar').animate({ 'width': '250px' }, speed);
            Cookie.set({name:'SidebarView', value:'On-SM'});
        } else {            
            Cookie.set({ name: 'SidebarView', value: 'On' });
            $('.sidebar').animate({ 'width': '250px' }, speed);
        }
        setTimeout(() => { $('.app-name').text(App.Info.Name); }, speed);
    }
    static sidebarOff(speed = 500) {
        if ($(window).width() <= 1000) {
            $('.sidebar').animate({ 'width': '0px' }, speed);
            Cookie.set({name:'SidebarView', value:'Off-SM'});
        } else {
            Cookie.set({name:'SidebarView', value:'Off'});
            $('.sidebar').animate({ 'width': '50px' }, speed);
        }
        $('.app-name').text(App.Info.Code);
    }
    static setPermission() {
        //By Pass Sys Admin
        if (App.User.Type == App.Setting.UserType.SysAdmin) return;
        //Get Form Name FromUrl
        let ApiName = window.location.pathname.replace('/');
        //Get Form Permission from session storage
        let Api = App.User.Api.find(x => x.ApiName == ApiName);
        if (Api == undefined) {
            Message.show({ status: Message.Type.error, statusText: `You are not authorised to access ${ApiName} page.` });
            setTimeout(() => {
                location.replace("/");
            }, 1000);
            return;
        }
        //Set Remove Buttons If Permission did not find
        if (!Api.View) {
            $('[data-action="View"]').remove();
        }
        if (!Api.Print) {
            $('[data-action="Print"]').remove();
        }
        if (!Api.Export) {
            $('[data-action="Export"]').remove();
        }
        if (!Api.Import) {
            $('[data-action="Import"]').remove();
        }
        if (!Api.Add) {
            $('[data-action="Add"]').remove();
        }
        if (!Api.Duplicate) {
            $('[data-action="Duplicate"]').remove();
        }
        if (!Api.Update) {
            $('[data-action="Edit"]').remove();
        }
        if (!Api.Delete) {
            $('[data-action="Delete"]').remove();
        }
        if (!Api.Enable) {
            $('[data-action="Enable"]').remove();
        }        
    }
    static setPage() {
        let title = $('title').text().split('-');
        $('title').text(`${App.Info.Name} - ${title[title.length - 1]}`);
        //Set Theme Color
        $('.page-body').attr('data-theme', App.User.Theme);
        //Side Bar
        $('.app-name').text(App.Info.Name);
        $('.user-name').text(App.User.Name);
        $('.user-deparment').text(App.User.Department);                        
        $('.user-name-first-char').text(App.User.Name.substring(0, 1));
        //Set Sidebar Status On Or Off
        if ($(window).width() > 1000) {
            if (Cookie.get('SidebarView') == 'Off') {
                Layout.sidebarOff(0);
            }
            else {
                Layout.sidebarOn(0);
            }
        }
        $('#appPage').removeClass('hide');
        Layout.runProgressBar();
    }
    static setMenu() {
        $('.sidebar-body .menu').html(App.AppMenu);
        //Menu Item Click
        $('.menu-item').on('click', (e) => {
            let slidSpeed = 400;
            let targetId = $(e.currentTarget).attr('data-target');
            let parentId = $(e.currentTarget).parents(':eq(1)').attr('id');
            $(".menu .root ul").not('#' + parentId + ',' + targetId).prev('a.menu-item').find('.fa.fa-angle-right').css({ "transform": "rotate(0deg)" });
            if (typeof (targetId) != 'undefined') {
                if ($(targetId).css('display') == 'block') {
                    $(targetId).slideUp(slidSpeed);
                    $(e.currentTarget).find('.fa.fa-angle-right').css({ "transform": "rotate(0deg)" });
                }
                else {
                    $(targetId).slideDown(slidSpeed);
                    $(e.currentTarget).find('.fa.fa-angle-right').css({ "transform": "rotate(90deg)" });
                }
                if (typeof (parentId) != 'undefined') {
                    $('.sidebar .menu .child').not('#' + parentId + ',' + targetId).slideUp(slidSpeed);
                }
                else {
                    $('.sidebar .menu .child').not(targetId).slideUp(slidSpeed);
                }
            }
        });
    }    
}