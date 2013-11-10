//----------------------------------------------------------
// Copyright (C) WebTimer LLC. All rights reserved.
//----------------------------------------------------------
// google-analytics.js
// Google Analytic Events 
var _gaq = _gaq || [];       // global Google Analytics Queue (required for async Events to work)
Events = {};

Events.googleAnalyticsNewScriptUri = '//www.google-analytics.com/analytics.js';
Events.googleAnalyticsOldScriptUri = '.google-analytics.com/ga.js';
Events.googleAnalyticsAppID = 'UA-44959542-1';

Events.EnableNew = function Events$EnableNew() {
    (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
        (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
        m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
    })(window,document,'script',Events.googleAnalyticsScriptUri,'ga');

    ga('create', Events.googleAnalyticsAppID, 'webtimer.co');
    ga('send', 'pageview');
    Events.enabled = true;
}

Events.PushNew = function (event) {
    if (Events.enabled == true) { ga(event); }
}
Events.TrackNew = function (category, action, optional) {
    if (Events.enabled == true) {
        if (optional != null) { ga(['send', 'event', category, action, optional]); }
        else { ga(['send', 'event', category, action]); }
    }
}

Events.EnableOld = function Events$EnableOld() {
    // enable Google Analytics
    var $plugin = $('<script type="text/javascript" async="true"> </script>');
    var src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + Events.googleAnalyticsOldScriptUri;
    $plugin.attr('src', src);
    $('script').first().parent().prepend($plugin);

    Events.enabled = true;
    Events.Push(['_setAccount', Events.googleAnalyticsAppID]);
    Events.Push(['_trackPageview']);
}

Events.PushOld = function (event) {
    if (Events.enabled == true) { _gaq.push(event); } 
}
Events.TrackOld = function (category, action, optional) {
    if (Events.enabled == true) {
        if (optional != null) { _gaq.push(['_trackEvent', category, action, optional]); }
        else { _gaq.push(['_trackEvent', category, action]); }
    }
}

// set to old google analytics
Events.googleAnalyticsScriptUri = Events.googleAnalyticsOldScriptUri;
Events.Enable = Events.EnableOld;
Events.Push = Events.PushOld;
Events.Track = Events.TrackOld;

Events.Categories = {};

// Category names
Events.Categories.LandingPage = 'LandingPage';
Events.Categories.AboutPage = 'AboutPage';
Events.Categories.Header = 'Header';
Events.Categories.Footer = 'Footer';
Events.Categories.Wizard = 'Wizard';
Events.Categories.LoginMenu = 'LoginMenu';
Events.Categories.DashboardHeader = 'DashboardHeader';
Events.Categories.DashboardToolbar = 'DashboardToolbar';
Events.Categories.DashboardDetail = 'DashboardDetail';
Events.Categories.People = 'People';
Events.Categories.Devices = 'Devices';

// Landing page actions
Events.LandingPage = {};
Events.LandingPage.SignUpButton = 'SignUpButton';
Events.LandingPage.SignUpFormPost = 'SignUpFormPost';
Events.LandingPage.SignInButton = 'SignInButton';
Events.LandingPage.SignInFormPost = 'SignInFormPost';
Events.LandingPage.LearnMoreButton = 'LearnMoreButton';
Events.LandingPage.DownloadButton = 'DownloadButton';
Events.LandingPage.BigSignUpButton = 'BigSignUpButton';

// Header actions
Events.Header = {};
Events.Header.HomeButton = 'HomeButton';
Events.Header.DownloadButton = 'DownloadButton';

// Footer actions
Events.Footer = {};
Events.Footer.AboutButton = 'AboutButton';
Events.Footer.PricingButton = 'PricingButton';
Events.Footer.ContactButton = 'ContactButton';
Events.Footer.TermsButton = 'TermsButton';
Events.Footer.PrivacyButton = 'PrivacyButton';
Events.Footer.FacebookButton = 'FacebookButton';
Events.Footer.TwitterButton = 'TwitterButton';
Events.Footer.CopyButton = 'CopyrightButton';

// Wizard actions
Events.Wizard = {};

// Login menu actions
Events.LoginMenu = {};
Events.LoginMenu.DashboardButton = 'DashboardButton';
Events.LoginMenu.SettingsButton = 'SettingsButton';
Events.LoginMenu.SignOutButton = 'SignOutButton';
Events.LoginMenu.HelpButton = 'HelpButton';

// Dashboard actions
Events.DashboardHeader = {};
Events.DashboardHeader.Dashboard = 'Dashboard';
Events.DashboardHeader.People = 'People';
Events.DashboardHeader.Devices = 'Devices';

// Dashboard toolbar actions
Events.DashboardToolbar = {};
Events.DashboardToolbar.Day = 'Day';
Events.DashboardToolbar.Week = 'Week';
Events.DashboardToolbar.Month = 'Month';
Events.DashboardToolbar.RefreshButton = 'RefreshButton';
Events.DashboardToolbar.Now = 'Now';
Events.DashboardToolbar.Forward = 'Forward';
Events.DashboardToolbar.Back = 'Back';
Events.DashboardToolbar.All = 'All';
Events.DashboardToolbar.PersonFilter = 'PersonFilter';

// Dashboard Detail actions
Events.DashboardDetail = {};
Events.DashboardDetail.ChartZoomIn = 'ChartZoomIn';
Events.DashboardDetail.ChartZoomOut = 'ChartZoomOut';

// People actions
Events.People = {};
Events.People.Add = 'PeopleAdd';
Events.People.Delete = 'PeopleDelete';
Events.People.Change = 'PeopleChange';
Events.People.NameChange = 'PeopleNameChange';
Events.People.ColorChange = 'PeopleColorChange';
Events.People.IsChildChange = 'PeopleIsChildChange';
Events.People.BirthdateChange = 'PeopleBirthdateChange';

// Devices actions
Events.Devices = {};
Events.Devices.Delete = 'DevicesDelete';
Events.Devices.Change = 'DevicesChange';
Events.Devices.EnabledChange = 'DevicesEnabledChange';
Events.Devices.NameChange = 'DevicesNameChange';
Events.Devices.TypeChange = 'DevicesTypeChange';

