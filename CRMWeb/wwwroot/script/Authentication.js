document.addEventListener("DOMContentLoaded", () => {
    sessionStorage.clear();    
    $('title').text(`${App.Info.Name} - Login`);
    $('#AppName').text(App.Info.Desc);
    $('#AppDesc').text(App.Info.SubDesc);
    $('#LoginInfo').removeClass('hide');
    Authentication.appInfo();
    Authentication.init();    
});

class Authentication {
    static init() {        
        Util.init();        
        $('#btnLogin').on('click', function () {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            var obj = {
                UserId: $('#UserId').val(),
                Password: $('#Password').val(),
                ApiType: App.Setting.ApiType.WebApp
            };
            Authentication.login(obj);
        });
    }
    static appInfo() {
        Data.get({
            url: 'Authentication/AppInfo', loader: true, onSuccess: (response) => {                
                App.Setting = response.data;                
                sessionStorage.setItem('Setting', JSON.stringify(response.data));
            }
        });
    }
    static login(obj) {        
        Data.post(
            {
                url: 'Authentication/Login',
                data: obj,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.data;
                        sessionStorage.setItem('User', JSON.stringify(obj.User));                        
                        sessionStorage.setItem('Company', JSON.stringify(obj.Company));
                        sessionStorage.setItem('AppMenu', Data.menuHtmlString(obj.AppMenu));                        
                        if (obj.AppMenu.length == 0 && obj.User.Type != App.Setting.UserType.SysAdmin) {
                            Message.show({ status: Message.Type.error, statusText: "You are not authorised to access this Application." });
                            return;
                        }
                        if ($(window).width() > 1000) {
                            Cookie.set({ name: 'SidebarView', value: "Off" });
                        }
                        location.replace(Url.App + "/Dashboard");
                    }
                    else {
                        Message.show(response);
                    }
                }
            },            
        );
    }
}

