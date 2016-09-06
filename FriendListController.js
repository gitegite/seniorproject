
var FriendApp = angular.module('FriendApp', []);


FriendApp.controller('FriendCtrl', ['$scope', '$http','$location', function ($scope, $http, $location) {

    $scope.friends = [];
    $scope.bothA = [];

    var friendlist = [];

    var studCode = sessionStorage.getItem('id').toString();
    studCode = studCode.substr(1);

    $scope.loading = true;
        $http({
            url: '/Friends/GetFacebookFriendList2JSON',
            method: "GET",
            params: { studentCode: studCode }
        }).then(function (success) {
            sessionStorage.friendA = JSON.stringify(success.data.friendList);
            $scope.bothA = success.data.friendList;
        }, function (error) {
            alert("error ja");
        });

    




    //var faceID = bothAService.getFaceID();

        //console.log($scope.friends);

    
        $http({
            url: '/Friends/GetFacebookFriendListJSON',
            method: "GET",
            params: { studentCode: studCode }
        }).then(function (success) {
            friendlist = success;
            $scope.friends = friendlist;
            sessionStorage.friendJSON = JSON.stringify(success);
            $scope.loading = false;
        }, function (error) {
            alert("error ja");
        });
    

    $scope.ChangeStatusA = function (studcode, facebookID) {
        $http({
            url: '/Friends/ChangeFriendStatus',
            method: "POST",
            data: {
                StudentCode: studcode,
                FacebookId: facebookID
            }
        }).then(function (success) {


        }, function (error) {
            alert("error ja");
        });
    };
    $scope.ChangeStatusB = function (studcode, facebookID) {
        $http({
            url: '/Friends/ChangeFriendStatusB',
            method: "POST",
            data: {
                StudentCode: studcode,
                FacebookId: facebookID
            }
        }).then(function (success) {

        }, function (error) {
            alert("error ja");
        });
    };
    $scope.update = function (key, status) {
        //console.log("Old: "+$scope.friends.data.friendList[key].FriendStatus);
        //$scope.friends.data.friendList[key].FriendStatus = status;
        //console.log("New: " + $scope.friends.data.friendList[key].FriendStatus);

        for (var i = 0; i < $scope.friends.data.friendList.length; ++i) {
            if ($scope.friends.data.friendList[i].StudentCode == key) {
                $scope.friends.data.friendList[i].FriendStatus = status;
            }
        }



    }
    $scope.viewFriendPlan = function (id) {
        sessionStorage.currentSeenFriendID = id;
        window.location = 'http://parazows.azurewebsites.net/FriendsPlan/Index?studentID=' + id;
    }

    $scope.bothAA = function (f) {
        $http({
            url: '/Friends/GetFriendStatus',
            method: "POST",
            data: {
                FriendStudID: f.StudentCode,
                MyFacebookID: faceID
            }
        }).then(function (success) {
            alert(success);
        }, function (error) {
            alert("error")
        });
    }

}]);

