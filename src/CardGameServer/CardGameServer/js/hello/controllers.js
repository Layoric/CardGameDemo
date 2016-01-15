/* global angular */
(function () {
    "use strict";

    var app = angular.module('helloApp.controllers', []);

    app.controller('helloCtrl', ['$scope', '$http',
        function ($scope, $http) {
            $scope.$watch('name', function () {
                if ($scope.name) {
                    $http.get('/hello/' + $scope.name)
                        .success(function (response) {
                            $scope.helloResult = response.Result;
                        });
                }
            });

            $scope.testFunction = function () {
                return true;
            };

            $scope.createGame = function() {
                $http.post('/ready', {}).then(function(response) {
                    console.log(response);
                    var source = new EventSource('/event-stream?channels=' + response.data.gameId + '&t=' + new Date().getTime()); //disable cache
                    source.addEventListener('error', function (e) {
                        console.log(e);
                    }, false);
                });
            };

            $scope.getAllGames = function() {
                $http.get('/blackjack/games').then(function (response) {
                    console.log(response.data);
                    $scope.allGames = response.data.allGames.games;
                });
            };

            $scope.getAllGames();
        }
    ]);
})();

