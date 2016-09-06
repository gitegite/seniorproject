var RecommenderApp = angular.module("RecommenderApp", []);

RecommenderApp.controller('RecommendCtrl', ['$scope', '$http', function ($scope, $http) {

    //var conflicts = sessionStorage.getItem('conflicts');

    //$scope.conflicts = conflicts.split(',');
    var semester = 1;
    var year = 2015;
    var studentCode = sessionStorage.getItem('id');
    studentCode = studentCode.substring(1, studentCode.length);
    $scope.conflicts = [];
    var noSeatGod = sessionStorage.noSeatGod;
    var courseList = noSeatGod.split(";");
    var color = ["#FF2929", "#FFC40F", "#3498DB", "#3DCE39", "#9B59B6", "#FF8200", "#FF1290", "#7F8C8D", "#D97F63"];
    var SelectSub = [];
    var SelectSec = [];
    for (var i = 0; i < courseList.length; i++) {
        var course = courseList[i].split(",");
        $scope.conflicts.push(course[0]);
    }

    var Input = sessionStorage.updatedGodSchedule.split(";");
    for (var i = 0 ; i < Input.length ; i++) {
        var course = Input[i].split(",");
        SelectSub.push(course[0]);
        SelectSec.push(course[1]);
    }
    $scope.colorValue = color;
    $scope.SelectSubjects = SelectSub;
    $scope.SelectSections = SelectSec;
    $scope.newSelectedGod = sessionStorage.updatedGodSchedule;
    $scope.editCourseCode = "";


    $http({
        url: '/Recommend/GetCourseListDetailJSON',
        method: "GET",
        params: {
            year: year,
            semester: semester,
            courseList: $scope.newSelectedGod
        }
    }).then(function (success) {
        $scope.newSelectedSchedule = success.data;

        var index = 0;
        var starttime = [0900, 1030, 1200, 1330, 1500, 1630, 1800, 1930, 2100];
        var startdiff = 0;
        var daynum = 1;
        var actualstart = 0;
        var colorIndex = 0;
        $scope.colorValue = color;

        var actualStarts = [];
        var startdiffList = [];
        var dayNumList = [];
        var usedTimeList = [];
        var colorList = [];

        console.log($scope.newSelectedSchedule);

        for (var k = 0 ; k < $scope.newSelectedSchedule.length ; k++) {
            var section = $scope.newSelectedSchedule[k].Sections[0];
            console.log(section);
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
                        if (sTime < starttime[j]) {
                            index -= 1;
                            startdiff = ((section.SectionDetails[i].StartTimeNum - starttime[index]) - (((section.SectionDetails[i].StartTimeNum - starttime[index]) / 100) * 40)) / 90;
                        }
                        actualStarts.push(index);
                        startdiffList.push(startdiff);
                        usedTimeList.push(subjectLength);
                        colorList.push(color[colorIndex]);
                        break;
                    }
                }
            }
            colorIndex += 1;
        }
        $scope.actualStart = actualStarts;
        $scope.dayNum = dayNumList;
        $scope.startdiff = startdiffList;
        $scope.usedTime = usedTimeList;
        $scope.usedColor = colorList;
    }, function (error) {
        alert("error ja");
    });
    


    $scope.getSections = function (coursecode) {
        $scope.selectedCourseCode = coursecode;
        var noSeatGod = sessionStorage.noSeatGod;
        var courseList = noSeatGod.split(";");
        var sectionNumber = "";
        for (var i = 0; i < courseList.length; i++) {
            var courseCode = courseList[i].split(",");
            if (courseCode[0] === coursecode) {
                sectionNumber = courseCode[1];
            }
        }

        var cuurentScheduleList = sessionStorage.updatedGodSchedule;
        $http({
            url: '/Recommend/SuggestSectionJSON',
            method: "GET",
            params: {
                studentCode: studentCode,
                year: year,
                semester: semester,
                scheduleList: cuurentScheduleList,
                courseCode: coursecode,
                sectionNumber: sectionNumber
            }
        }).then(function (success) {
            $scope.sections = success.data;
        }, function (error) {
            alert("error ja");
        });

    }

    $scope.drawSchedule = function (courseCode, sectionNumber) {
        
        SelectSub = [];
        SelectSec = [];
        sessionStorage.updateSubject = courseCode;
        var currentGod = sessionStorage.updatedGodSchedule.split(";");
        var newSelectedSection = courseCode + "," + sectionNumber + ";";
        var newGod = "";
        var CourseCodeList = [];
        var ToEditCourseCode = "";
        for (var i = 0; i < currentGod.length; i++) {
            var courseSection = currentGod[i].split(",");
            if (courseSection[0] !== courseCode) {
                SelectSub.push(courseSection[0]);
                SelectSec.push(courseSection[1]);
                ToEditCourseCode = courseCode;
                newGod += currentGod[i] + ";";
            }
            else {
                newGod += newSelectedSection;
                SelectSub.push(courseCode);
                SelectSec.push(sectionNumber);
            }
        }

        $scope.newSelectedGod = newGod.substring(0, newGod.length - 1);
        sessionStorage.updatedGodSchedule = $scope.newSelectedGod;
        $scope.SelectSubjects = SelectSub;
        $scope.SelectSections = SelectSec;
        

        $http({
            url: '/Recommend/GetCourseListDetailJSON',
            method: "GET",
            params: {
                year: year,
                semester: semester,
                courseList: $scope.newSelectedGod
            }
        }).then(function (success) {
            $scope.newSelectedSchedule = success.data;

            var index = 0;
            var starttime = [0900, 1030, 1200, 1330, 1500, 1630, 1800, 1930, 2100];
            var startdiff = 0;
            var daynum = 1;
            var actualstart = 0;
            var colorIndex = 0;
            $scope.colorValue = color;

            $scope.allDayNum = [1, 2, 3, 4, 5, 6, 7];

            var actualStarts = [];
            var startdiffList = [];
            var dayNumList = [];
            var usedTimeList = [];
            var colorList = [];
            var SubjectN = [];

            console.log($scope.newSelectedSchedule);

            for (var k = 0 ; k < $scope.newSelectedSchedule.length ; k++) {
                SubjectN.push($scope.newSelectedGod[k].courseName);
                var section = $scope.newSelectedSchedule[k].Sections[0];
                console.log(section);
                for (var i = 0; i < section.SectionDetails.length; i++) {
                    var sTime = section.SectionDetails[i].StartTimeNum;
                    CourseCodeList.push(section.SectionDetails[i].courseName);
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
                            if (sTime < starttime[j]) {
                                index -= 1;
                                startdiff = ((section.SectionDetails[i].StartTimeNum - starttime[index]) - (((section.SectionDetails[i].StartTimeNum - starttime[index]) / 100) * 40)) / 90;
                            }
                            actualStarts.push(index);
                            startdiffList.push(startdiff);
                            usedTimeList.push(subjectLength);
                            colorList.push(color[colorIndex]);
                            break;
                        }
                    }
                }
                colorIndex += 1;
            }
            $scope.actualStart = actualStarts;
            $scope.dayNum = dayNumList;
            $scope.startdiff = startdiffList;
            $scope.usedTime = usedTimeList;
            $scope.usedColor = colorList;
            $scope.SelectSubjects = SelectSub;
            $scope.SelectSections = SelectSec;
            $scope.editCourseCode = ToEditCourseCode;
            $scope.subjectList = CourseCodeList;

        }, function (error) {
            alert("error ja");
        });
           
       
    }

    $scope.registerSchedule = function () {
        var godSchedule = sessionStorage.updatedGodSchedule;
        $http({
            url: '/Recommend/RegisterSchedule',
            method: "GET",
            params: {
                studentCode: studentCode,
                year: year,
                semester: semester,
                godSchedule: godSchedule
            }
        }).then(function (success) {
            var result = success.data;
            var resultMessage = result.split("\n");
            console.log(result);
            console.log(resultMessage);
            var registrationResult = "";
            if (resultMessage[0] === "Success") {
                alert("Succeeded");
            }
            else if (resultMessage[0] === "Fail") {
                var noSeatGod = "";
                registrationResult = "You need to select other sections for the following course(s)";
                noSeatGod = resultMessage[1];
                sessionStorage.noSeatGod = noSeatGod;
                sessionStorage.godSchedule = resultMessage[2];
                sessionStorage.updatedGodSchedule = resultMessage[2];
                alert(registrationResult);
                window.location.href = '/Recommend';
            }

        }, function (error) {
            alert("error ja");
        });

    }



}]);