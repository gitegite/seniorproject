function sortMethod(a, b) {
    var x = a.name.toLowerCase();
    var y = b.name.toLowerCase();
    return ((x > y) ? -1 : ((x > y) ? 1 : 0));
}

function getFriend() {
    FB.api('v2.3/me/friends', function (response) {
        for (var i = 0 ; i < response.data.length ; i++) {
            console.log(response.data[i].id + " " + response.data[i].name);
        }
    });
}

function statusChangeCallback(response) {
    console.log('statusChangeCallback');
    console.log(response);
    if (response.status === 'connected') {
        testAPI();
        getFriend();
    } else if (response.status === 'not_authorized') {
        document.getElementById('status').innerHTML = 'Please log ' +
          'into this app.';
    } else {
        document.getElementById('status').innerHTML = 'Please log ' +
          'into Facebook.';
    }
}

function checkLoginState() {
    FB.getLoginStatus(function (response) {
        statusChangeCallback(response);
    });
}

window.fbAsyncInit = function () {
    FB.init({
        appId: '1598328510380395',
        xfbml: true,
        version: 'v2.4'
    });

    FB.getLoginStatus(function (response) {
        statusChangeCallback(response);
    });
};

(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement(s); js.id = id;
    js.src = "//connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));

function testAPI() {
    console.log('Welcome!  Fetching your information.... ');
    FB.api('/me', function (response) {
        console.log('Successful login for: ' + response.name);
        document.getElementById('status').innerHTML =
          'Thanks for logging in, ' + response.name + '!';
    });
}