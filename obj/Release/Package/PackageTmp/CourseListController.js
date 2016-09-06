

var CourseListApp = angular.module("CourseListApp", []);

CourseListApp.directive('modal', function () {
    return {
        template: '<div class="modal fade">' +
            '<div class="modal-dialog">' +
              '<div class="modal-content">' +
                '<div class="modal-header">' +
                  '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>' +
                  '<h4 class="modal-title">{{ title }}</h4>' +
                '</div>' +
                '<div class="modal-body" ng-transclude></div>' +
              '</div>' +
            '</div>' +
          '</div>',
        restrict: 'E',
        transclude: true,
        replace: true,
        scope: true,
        link: function postLink(scope, element, attrs) {
            scope.title = attrs.title;

            scope.$watch(attrs.visible, function (value) {
                if (value == true)
                    $(element).modal('show');
                else
                    $(element).modal('hide');
            });

            $(element).on('shown.bs.modal', function () {
                scope.$apply(function () {
                    scope.$parent[attrs.visible] = true;
                });
            });

            $(element).on('hidden.bs.modal', function () {
                scope.$apply(function () {
                    scope.$parent[attrs.visible] = false;
                });
            });
        }
    };
});

CourseListApp.controller("CourseListCtrl", ['$scope', '$http', function ($scope, $http) {
    var courselist = []
    var studCode = sessionStorage.getItem('id').toString();

    if (sessionStorage.courseList == null) {
        $scope.courselist = [];
        $http({
            url: '/SectionSelector/GetStudentCourseJSON',
            method: "GET",
            params: { year: '2015', semester: '1' }
        }).then(function (success) {
            sessionStorage.courseList = JSON.stringify(success);
            courselist = success;
            $scope.courselist = courselist;
        }, function (error) {
            alert("error ja");
        });
    }
    else {
        $scope.courselist = JSON.parse(sessionStorage.courseList);
    }

    $scope.sectionlist = [];

    $scope.showSection = function (coursecode, credit) {
        $scope.creditSum = credit;
        $http({
            url: '/SectionSelector/ShowSectionJSON',
            method: "GET",
            params: {
                year: '2015',
                semester: '1',
                courseCode: coursecode,
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



        }, function (error) {
            alert("error ja");
        });

    }

    $scope.selectedSections = [];
    $scope.selectAll = function () {

    }
    $scope.confirm = function () {
        $scope.selectedSections = [];
        var sectionsNum = "";

        angular.forEach($scope.sectionlist, function (item) {
            if (item.isChecked) {
                sectionsNum += item.SectionNumber + ",";
                $scope.selectedSections.push(item);
            }
        });
        var courseName = $scope.selectedSections[0].CourseName;
        var courseCode = $scope.selectedSections[0].CourseCode;
        sectionsNum = sectionsNum.substr(0, sectionsNum.length - 1);

        $http({
            url: '/SectionSelector/AddSubject',
            method: "POST",
            params: {
                CourseCode: courseCode,
                selectedsections: sectionsNum
            }
        }).then(function (success) {

            if (sessionStorage.getItem("coursecode") == "") {
                sessionStorage.setItem("coursecode", courseCode);
                sessionStorage.setItem("coursename", courseName);
            } else {

                var temp = sessionStorage.getItem("coursecode").toString().split(',');
                var isExisted = false;
                for (var i = 0 ; i < temp.length; ++i) {
                    if (temp[i] == courseCode) {
                        isExisted = true;
                        break;
                    }

                }

                if (!isExisted) {
                    sessionStorage.setItem("coursecode", sessionStorage.getItem('coursecode') + "," + courseCode);
                    sessionStorage.setItem("coursename", sessionStorage.getItem('coursename') + "," + courseName);
                }
            }
            sessionStorage.setItem("god", success.data);
            location.href = "/AddCourse";

        }, function (error) {
            alert("error ja");
        });


    }
    $scope.isVisible = false;
    $scope.allChecked = false;

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
                    var ST = new Date(1, 1, 1,Math.floor(section.SectionDetails[i].StartTimeNum/100) , Math.floor(section.SectionDetails[i].StartTimeNum%100),0,0);
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

    $scope.selectall = function () {

        if (!$scope.allChecked) {
            angular.forEach($scope.sectionlist, function (item) {
                item.isChecked = true;
                $scope.allChecked = true;
            });
        } else {
            angular.forEach($scope.sectionlist, function (item) {
                item.isChecked = false;
                $scope.allChecked = false;
            });
        }
    }

}]);