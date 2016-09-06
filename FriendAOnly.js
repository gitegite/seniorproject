
var FriendApp = angular.module('FriendApp', []);


FriendApp.controller('FriendCtrl', ['$scope', '$http', function ($scope, $http) {

    var studCode = sessionStorage.getItem('id').toString();
    studCode = studCode.substr(1);
    $scope.bothAA = [];
    $scope.loading = true;
        $http({
            url: '/Friends/GetFacebookFriendList2JSON',
            method: "GET",
            params: { studentCode: studCode }
        }).then(function (success) {
            sessionStorage.friendA = JSON.stringify(success.data.friendList);
            $scope.bothAA = success.data.friendList;
            $scope.loading = false;
        }, function (error) {
            alert("error ja");
        });

    

    

    sessionStorage.friendStudentCodes = "";
    $scope.hasConfirmed = false;
    $("#pickfribtn").click(function () {
        $scope.hasConfirmed = false;

    });
    
    $scope.setFriendStudentCode = function () {
        $scope.checkedFriends = [];

        var studentCodes = "";
        angular.forEach($scope.bothAA, function (value, key) {

            if (value.IsChecked) {
                studentCodes += value.StudentCode + ",";
                $scope.checkedFriends.push(value);
            }

        });
        sessionStorage.friendStudentCodes = studentCodes.substr(0, studentCodes.length - 1);
        $scope.friendStudentCodes = sessionStorage.friendStudentCodes;
        $scope.hasConfirmed = true;

        $('#myModal').modal('hide');
    }
    var studCode = sessionStorage.getItem('id').toString();
    studCode = studCode.substr(1);

    $scope.sectionlist = [];
    $scope.getSections = function (coursecode) {
        $http({
            url: '/SectionSelector/ShowSectionJSON',
            method: "GET",
            params: {
                year: '2015',
                semester: '1',
                courseCode: coursecode.trim(),
                studentCode: studCode
            }
        }).then(function (success) {
            $scope.sectionlist = success.data;
            $scope.allDayAvailable = [1, 2, 3, 4, 5, 6, 7];

            $scope.allStudyDayNum = [];

            for (var i = 0; i < $scope.sectionlist.length; i++) {
                var studyDayNum = [0, 0, 0, 0, 0, 0, 0];
                var detail = $scope.sectionlist[i];
                for (var j = 0; j < detail.SectionDetails.length; j++) {
                    studyDayNum[detail.SectionDetails[j].DayNum - 1] = detail.SectionDetails[j].DayNum;
                    if (!detail.SectionDetails[j].IsMorning) {
                        studyDayNum[detail.SectionDetails[j].DayNum - 1] += 7;
                    }
                }
                $scope.allStudyDayNum.push(studyDayNum);
            }


            var god = sessionStorage.getItem('god').toString();
            god = god.split(';');
            var coursecode = $scope.sectionlist[0].CourseCode;
            var sections;
            for (var i = 0 ; i < god.length; ++i) {
                if (god[i].indexOf(coursecode) > -1) {
                    sections = god[i];
                    break;
                }
            }
            sections = sections.trim().split(',');
            sections = sections.splice(1);
            console.log(sections);

            angular.forEach($scope.sectionlist, function (value, key) {
                if (sections.indexOf(value.SectionNumber.toString()) > -1) {
                    value.isChecked = true;
                } else {
                }
            });

        }, function (error) {

        });


        $('#sectionNumModal').modal('show');
    }
    $scope.showDetail = function (section, sectionNumber) {

        var index = 0;
        var starttime = [0900, 1030, 1200, 1330, 1500, 1630, 1800, 1930, 2100];
        var startdiff = 0;
        var daynum = 1;
        var actualstart = 0;

        $scope.detail = section;
        $scope.isVisible = !$scope.isVisible;
        $scope.sectionNumber = sectionNumber;
        $scope.allDayNum = [1, 2, 3, 4, 5, 6, 7];

        var actualStarts = [];
        var startdiffList = [];
        var dayNumList = [];
        var usedTimeList = [];

        for (var i = 0; i < section.SectionDetails.length; i++) {
            var sTime = section.SectionDetails[i].StartTimeNum;
            dayNumList.push(section.SectionDetails[i].DayNum);
            var index = 0;
            var subjectLength = 0;
            var startdiff = 0;

            for (var j = 0 ; j < starttime.length ; j++) {
                if (sTime <= starttime[j]) {
                    index = j;
                    var ST = new Date(1, 1, 1, Math.floor(section.SectionDetails[i].StartTimeNum / 100), Math.floor(section.SectionDetails[i].StartTimeNum % 100), 0, 0);
                    var ET = new Date(1, 1, 1, Math.floor(section.SectionDetails[i].EndTimeNum / 100), Math.floor(section.SectionDetails[i].EndTimeNum % 100), 0, 0);
                    var diffMs = ET - ST;
                    subjectLength = Math.floor(diffMs / 1000 / 60);
                    console.log(ET);
                    console.log(ST);
                    console.log(diffMs);
                    if (sTime < starttime[j]) {
                        index -= 1;
                        startdiff = ((section.SectionDetails[i].StartTimeNum - starttime[index]) - (((section.SectionDetails[i].StartTimeNum - starttime[index]) / 100) * 40)) / 90;
                    }
                    actualStarts.push(index);
                    startdiffList.push(startdiff);
                    usedTimeList.push(subjectLength);
                    break;
                }
            }
        }
        $scope.actualStart = actualStarts;
        $scope.dayNum = dayNumList;
        $scope.startdiff = startdiffList;
        $scope.usedTime = usedTimeList;
    }

    $scope.confirm = function () {

        var god = sessionStorage.getItem('god').toString();
        god = god.split(';');
        console.log(god);
        var coursecode = $scope.sectionlist[0].CourseCode;
        var index = 0;


        var newSections = coursecode + ",";
        var unchanged = true;
        angular.forEach($scope.sectionlist, function (value, key) {
            if (value.isChecked) {
                newSections += value.SectionNumber + ",";
                unchanged = false;
            }
        });
        newSections = newSections.substr(0, newSections.length - 1);
        if (unchanged == false) {
            for (var i = 0; i < god.length; ++i) {
                if (god[i].indexOf(coursecode) > -1) {
                    god[i] = newSections;
                    break;
                }
            }
            god = god.join(';');
            sessionStorage.setItem('god', god);
            console.log(god);
        } else {
            god = god.join(';');
            alert('No section changed!');
            console.log(god);
        }
        $('#sectionNumModal').modal('hide');

    }
    $scope.showMoreFriends = function () {
        $("#moreFriendsModal").modal('show');

    }
    $scope.selectAll = function () {
        angular.forEach($scope.bothAA, function (value, key) {

            value.IsChecked = true;

        });
    }
}]);

