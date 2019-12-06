///// <amd-dependency path='app/model/service' />
//import Service from 'app/model/service';
//import AjaxRedirect from 'olive/mvc/ajaxRedirect';
//import ErrorViewsNavigator from 'app/error/errorViewsNavigator';
//export default class FeaturesMenuNavigationManager {
//    public static navigationFailed(url: string, response: JQueryXHR) {
//        let service = Service.fromUrl(url);
//        if (service)
//            ErrorViewsNavigator.goToServiceError(service, url);
//        else
//            AjaxRedirect.defaultOnRedirectionFailed(url, response);
//    }
//}
//# sourceMappingURL=featuresMenuNavigationManager.js.map