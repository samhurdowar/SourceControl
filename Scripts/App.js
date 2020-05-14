
var app = angular.module('MainApp', ['ngTouch', 'ngSanitize', 'ngCookies', 'ngAnimate', 'ui.grid', 'ui.bootstrap', 'ui.grid.resizeColumns', 'ui.grid.moveColumns', 'ui.grid.selection', 'ui.grid.exporter']);

app.controller('MainCtrl', ['fwService', '$scope', '$compile',
	function (fwService, $scope, $compile) {

		$scope.TriggerAngular = function () {
			var html = AngularTemplateBody;

			var compiledHtml = $compile(html)($scope);
			angular.element(document.getElementById('AngularTemplatePlaceHolder666')).append(compiledHtml);


			fwService.fwSummary().then(function (d) {
				$scope.fwList = d.data.fwList;
			},
				function (error) {
					alert('Error getting summary information!');

				}
			);
		}

	}
]);

app.controller('GroupsController', ['GroupsService', '$scope', '$compile',
	function (GroupsService, $scope, $compile) {

		$scope.groupSelected = [];
		$scope.location = '';
		$scope.groupguid = '';
		$scope.newGroupName = '';
		$scope.devicesNotinGroup = [];
		$scope.newGroupAddMember = 'false';
		$scope.groupSelected.GroupName = 'root';
		$scope.isCollapsed = false;
		$scope.groupSelected.GroupID = 0;
		$scope.SelectedDevices = [];
		$scope.deviceIDs = [];
		$scope.dropdownSetting = {
			scrollableHeight: '200px',
			scrollable: true,
			enableSearch: true
		}

		$scope.TriggerAngular2 = function () {
			var html = AngularTemplateBody;
			var compiledHtml = $compile(html)($scope);
			angular.element(document.getElementById('AngularTemplatePlaceHolder777')).append(compiledHtml);

			GroupsService.GetDeviceList().then(function (d) {
				$scope.GroupList = d.data;
			}, function (error) {
				alert('Error! DeviceGroupsV2.js');
			});
		}


		$scope.showChilds = function (item) {
			$scope.grouplist = [];

			item.active = !item.active;
			//console.log("here item=" + item.GroupName + " active=" + item.GroupName.active);
			grouplist = item.SubGroup;

		}

		$scope.showInfoForGroup = function (item) {
			//console.log("item = " + item + "Count = " + item.DeviceCount);
			GroupsService.GetGroupDeviceInformation(item).then(function (d) {

				$scope.groupSelected = d.data.devicegroupitem;
				//$scope.devicesNotinGroup = d.data.devicesnotingroup;
				angular.forEach(d.data.devicesnotingroup, function (value, index) {
					$scope.devicesNotinGroup.push({ label: value.HostName, id: value.HostName });
				});
				$scope.newGroupAddMember = 'false';
			})

		}, function (error) {
			//console.log("item = " + item + "Count = " + groupSelected.DeviceCount);
		};

		$scope.DeleteDeviceFromGroup = function (deviceguid, groupguid) {
			//console.log("DeviceGuid = " + deviceguid + " GroupGuid = " + groupguid);
			GroupsService.DeleteDeviceFromList(deviceguid, groupguid).then(function (d) {
				$scope.groupSelected = d.data;
			})
		}

		$scope.AddDeviceToGroup = function (hostname, groupguid) {
			GroupsService.AddDeviceToGroup(hostname, groupguid).then(function (d) {
				$scope.newGroupAddMember = 'false';
				$scope.showInfoForGroup(dt.guid);
				//$scope.groupSelected = d.data.devicegroupitem;
				//$scope.devicesNotinGroup = d.data.devicesnotingroup;

			})
		}

		$scope.SubmitMultipleDevices = function (groupguid) {
			$scope.deviceIDs = [];
			//console.log($scope.SelectedDevices);
			angular.forEach($scope.SelectedDevices, function (value) {
				$scope.deviceIDs.push({ dname: value.id, dguid: groupguid });
			});

			var data = { deviceIDs: $scope.deviceIDs };
			//console.log(data);
			angular.toJson(data.deviceIDs);
			GroupsService.SubmitMultiDevicesToGroup(data.deviceIDs)
				.success(function () {
					$scope.devicesNotinGroup = [];
					$scope.SelectedDevices = [];
					$scope.showInfoForGroup(groupguid);
				})
				.error(function (error) {

				});

		}

		$scope.CreateGroup = function (groupID, groupName, newGroupAddMember) {
			angular.isUndefinedOrNull = function (groupID) {
				return angular.isUndefined(groupID) || groupID === null
			}
			$scope.GroupList = '';

			GroupsService.CreateSubGroup(groupID, groupName, newGroupAddMember).then(function (d) {
				$scope.GroupList = d.data;
				$scope.newGroupName = '';
				$scope.groupSelected.GroupName = null;
			})
		}

		$scope.DeleteGroup = function (groupID) {
			$scope.GroupList = '';
			GroupsService.DeleteSubGroup(groupID).then(function (d) {
				$scope.GroupList = d.data;
				$scope.newGroupName = '';
				$scope.groupSelected.GroupName = null;
			})

		}

	}
]);

app.factory('fwService', function ($http) {
	var fac = {};

	fac.fwSummary = function () {
		return $http.get("/SwitchPorts/FWRouteSummaryJSON");
	}

	return fac;
});

app.factory('GroupsService', function ($http) { // explained about factory in Part2
	var fac = {};
	fac.GetDeviceList = function () {
		return $http.get('/SwitchPorts/getgrouptree')
	}
	fac.GetGroupDeviceInformation = function (guid) {
		return $http.get('/SwitchPorts/GetGroupDeviceInfo?groupguid=' + guid)
	}
	fac.DeleteDeviceFromList = function (deviceguid, groupguid) {
		return $http.get('/SwitchPorts/DeleteDeviceFromGroup?deviceguid=' + deviceguid + "&groupguid=" + groupguid)
	}
	fac.AddDeviceToGroup = function (hostname, groupguid) {
		return $http.get('/SwitchPorts/AddDeviceToGroup?hostname=' + hostname + "&groupguid=" + groupguid)
	}
	fac.CreateSubGroup = function (groupID, groupName, newGroupAddMember) {
		return $http.get('/SwitchPorts/CreateGroup?GroupID=' + groupID + "&groupName=" + groupName + "&AddMember=" + newGroupAddMember)
	}
	fac.DeleteSubGroup = function (groupID) {
		return $http.get('/SwitchPorts/DeleteGroup?GroupID=' + groupID)
	}
	fac.SubmitMultiDevicesToGroup = function (data) {
		return $http.post('/SwitchPorts/AddMultipleDevicesToGroup', data)
	}
	return fac;
});

app.controller('VMSummaryController', ['VMSummaryService', '$scope', '$compile', '$sce', '$window', '$location',
	function (VMSummaryService, $scope, $compile, $sce, $window, $location) {
		$scope.loading = false;
		$scope.summaryData = [];
		$scope.dynamicPopover = [];
		$scope.Notifications = [];
		$scope.serverList = [];
		$scope.sortType = 'server';
		$scope.sortReverse = false;
		$scope.searchTerm = '';
		$scope.selectedServer = '';
		$scope.appList = [];

		$scope.TriggerAngular004 = function () {
			
			var html = AngularTemplateBody;
			var compiledHtml = $compile(html)($scope);
			angular.element(document.getElementById('AngularTemplatePlaceHolder004')).append(compiledHtml);


			VMSummaryService.NMSummary().then(function (d) {

				$scope.TotalServerCount = d.data.TotalServerCount;
				$scope.SolarisServerCount = d.data.SolarisServerCount;
				$scope.SolarisRecordCount = d.data.SolarisRecordCount;
				$scope.LinuxServerCount = d.data.LinuxServerCount;
				$scope.LinuxRecordCount = d.data.LinuxRecordCount;
				$scope.SolarisResolvedRecordCount = d.data.SolarisResolvedRecordCount;
				$scope.LinuxResolvedRecordCount = d.data.LinuxResolvedRecordCount;

				$scope.loading = false;
			},
				function (error) {
					alert('Error getting summary information!');
				});

		}

		$scope.shareMyData = function (myValue) {
			//xxx$cookies.put('sendServer', myValue);
			window.location.href = "/VulnManagement/WW_ManageServerData";
		};
		$scope.shareMyData2 = function (myValue) {
			//xxx$cookies.put('sendApp', myValue);
			window.location.href = "/VulnManagement/WW_ManageAppData";
		};



		$scope.loadServerList = function (d) {
			$scope.loading = true;
			VMSummaryService.getSummaryServerList().then(function (d) {
				$scope.serverList = d.data.serverList;
				$scope.loading = false;
			}, function (error) {
				alert('Error! VM-WW-Summary.js');
			});
		};


		$scope.loadAppList = function (d) {
			$scope.loading = true;
			VMSummaryService.getSummaryAppList().then(function (d) {
				$scope.appList = d.data.appList;
				$scope.loading = false;
			}, function (error) {
				alert('Error! VM-WW-Summary.js');
			});
		};

		$scope.placement = {
			options: [
				'top',
				'top-left',
				'top-right',
				'bottom',
				'bottom-left',
				'bottom-right',
				'left',
				'left-top',
				'left-bottom',
				'right',
				'right-top',
				'right-bottom'
			],
			selected: 'top'
		};

		$scope.dynamicPopover = {
			Description: 'Description',
			templateUrl: '/VulnManagement/myPopoverTemplate',
			Name: 'Title',
			PluginID: 'pluginID'
		};

		$scope.changeDynamicContent = function (data) {
			$scope.dynamicPopover.Description = data.Description;
			$scope.dynamicPopover.Name = data.PluginName;
			$scope.dynamicPopover.PluginID = data.PluginID;
		};


	}]);


app.controller('f5ManageJobsController', ['f5Service', '$anchorScroll', '$location', '$scope', '$compile', '$log', '$uibModal',
	function (f5Service, $anchorScroll, $location, $scope, $compile, $log, $uibModal) {
		
		$scope.guidList = [];
		$scope.selectedDate = new Date();
		$scope.selectedDateOpen = false;

		$scope.TriggerAngular5 = function () {
			var html = AngularTemplateBody;
			var compiledHtml = $compile(html)($scope);
			angular.element(document.getElementById('AngularTemplatePlaceHolder000')).append(compiledHtml);

			// Populate Jobs
			f5Service.GetJobs().then(function (d) {
				$scope.JobsList = {};
				$scope.JobsList = d.data;
			}, function (error) {
				alert('Error! f5ManageJobs.js');
			});
		}

		$scope.ChangeJobState = function (job) {
			if (job.show == true) {
				job.show = false;
				var index = $scope.guidList.indexOf(job.guid);
				$scope.guidList.splice(index, 1);
			}
			else {
				job.show = true;
				$scope.guidList.push(job.guid);
			}
			console.log($scope.guidList);


		};

		$scope.DeleteRecordsByGuids = function () {
			var data = angular.toJson($scope.guidList);
			f5Service.deletejobsbyguids(data).then(function (d) {
				$scope.JobsList.length = 0;
				$scope.JobsList = d.data;
				$scope.guidList.length = 0;
			},

				function (error) {
					alert('Error Device!');
				});
		};

		$scope.DeleteRecordsByDate = function (date) {

			f5Service.deletejobsbyDate(date).then(function (d) {
				$scope.JobsList.length = 0;
				$scope.JobsList = d.data;
			},

				function (error) {
					alert('Error Device!');
				});
		};

		$scope.opened = {};

		$scope.open = function ($event, dt) {
			$event.preventDefault();
			$event.stopPropagation();

			dt.opened = true;
		};

		$scope.open2 = function ($event, dt) {
			$event.preventDefault();
			$event.stopPropagation();

			dt.opened2 = true;
		};


		$scope.today = function () {
			$scope.dt = new Date();
		};

		$scope.today();

		$scope.clear = function () {
			$scope.dt = null;
		};

		$scope.inlineOptions = {
			customClass: getDayClass,
			minDate: new Date(),
			showWeeks: true
		};

		$scope.dateOptions = {
			dateDisabled: disabled,
			formatYear: 'yy',
			maxDate: new Date(2020, 5, 22),
			minDate: new Date(),
			startingDay: 1
		};

		// Disable weekend selection
		function disabled(data) {
			//var date = data.date,
			//  mode = data.mode;
			//return mode === 'day' && (date.getDay() === 0 || date.getDay() === 6);

		}

		$scope.toggleMin = function () {
			$scope.inlineOptions.minDate = $scope.inlineOptions.minDate ? null : new Date();
			$scope.dateOptions.minDate = $scope.inlineOptions.minDate;
		};

		$scope.toggleMin();



		$scope.setDate = function (year, month, day) {
			$scope.dt = new Date(year, month, day);
		};

		$scope.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
		$scope.format = $scope.formats[0];
		$scope.altInputFormats = ['M!/d!/yyyy'];

		$scope.popup1 = {
			opened: false
		};

		$scope.popup2 = {
			opened: false
		};

		var tomorrow = new Date();
		tomorrow.setDate(tomorrow.getDate() + 1);
		var afterTomorrow = new Date();
		afterTomorrow.setDate(tomorrow.getDate() + 1);
		$scope.events = [
			{
				date: tomorrow,
				status: 'full'
			},
			{
				date: afterTomorrow,
				status: 'partially'
			}
		];

		function getDayClass(data) {
			var date = data.date,
				mode = data.mode;
			if (mode === 'day') {
				var dayToCheck = new Date(date).setHours(0, 0, 0, 0);

				for (var i = 0; i < $scope.events.length; i++) {
					var currentDay = new Date($scope.events[i].date).setHours(0, 0, 0, 0);

					if (dayToCheck === currentDay) {
						return $scope.events[i].status;
					}
				}
			}

			return '';
		};
	}])


app.controller('f5DevicesController', ['f5Service', '$anchorScroll', '$location', '$scope', '$compile', '$log', '$uibModal',
	function (f5Service, $anchorScroll, $location, $scope, $compile, $log, $uibModal) {
		$scope.loading = false;
		$scope.jobguid = null;
		$scope.f5hostname = null;
		$scope.appGroup = null;
		$scope.selectedDevice = null;
		$scope.selectedVIP = null;
		$scope.CertList = [];
		$scope.searchString = "";
		$scope.downVIPList = [];
		$scope.OfflineVIPs = [];
		$scope.sentlable = false;

		$scope.TriggerAngular6 = function () {
			var html = AngularTemplateBody;
			var compiledHtml = $compile(html)($scope);
			angular.element(document.getElementById('AngularTemplatePlaceHolder001')).append(compiledHtml);

			f5Service.f5Summary().then(function (d) {

				$scope.poolcount = d.data.poolcount;
				$scope.rulecount = d.data.rulecount;
				$scope.devicecount = d.data.devicecount;
				$scope.activePoolMembers = d.data.activePoolMembers;
				$scope.totalPoolMembers = d.data.totalPoolMembers;
				$scope.offlineVipCount = d.data.offlineVipCount;
				$scope.unknownVipCount = d.data.unknownVipCount;
				$scope.availableVipCount = d.data.availableVipCount;
				$scope.totalVIPCount = d.data.totalVIPCount;
				$scope.expiringSixMonthCount = d.data.expiringSixMonthCount;
				$scope.expiringThreeMonthCount = d.data.expiringThreeMonthCount;
				$scope.expiringOneMonthCount = d.data.expiringOneMonthCount;
				$scope.expiredCertCount = d.data.expiredCertCount;
				$scope.TotalCertCount = d.data.TotalCertCount;
				$scope.loading = false;
			},
				function (error) {
					alert('Error getting summary information!');
				});

		}


		$scope.EmailCertData = function () {
			f5Service.EmailCertData().then(function (d) {
				$scope.sentlable = true;
			})
		};


	}])


app.controller('VMController', ['VulnService', '$scope', '$compile', '$log', '$interval', '$timeout', '$filter', '$sce', function (VulnService, $scope, $compile, $log, $interval, $timeout, $filter, $sce) {
	$scope.loading = false;
	
	$scope.selectedReport = [];
	$scope.reportItems = [];
	$scope.severities = [{ "value": 0, "text": "all" }, { "value": 1, "text": "High & Critical" }, { "value": 2, "text": "Medium & Low" }];
	$scope.selectedSev = 5;
	$scope.sortType = 'name';
	$scope.sortReverse = false;
	$scope.searchTerm = '';
	$scope.selectedGroup = '';
	$scope.dynamicPopover = [];

	$scope.TriggerAngular003 = function () {
		var html = AngularTemplateBody;
		var compiledHtml = $compile(html)($scope);
		angular.element(document.getElementById('AngularTemplatePlaceHolder003')).append(compiledHtml);

		VulnService.reportList().then(function (d) {
			$scope.reportsList = [];
			$scope.groupList = {};
			$scope.loading = true;
			$scope.reportsList = d.data.reportsList;
			$scope.groupList = d.data.groupList;
			$scope.loading = false;
		},
			function (error) {
				alert('Error getting summary information!');
			}
		);

	}


	$scope.getReportDetails = function (report) {
		$scope.loading = true;
		VulnService.getNessus5ReportItemsJSON(report, $scope.selectedSev).then(function (d) {
			$scope.reportItems = d.data.reportData;
			$scope.loading = false;
		}, function (error) {
			alert('Error! Nessus5Reports.js');
		});
	};

	$scope.reset = function () {
		$scope.reportItems = [];
		$scope.selectedSev = null;
	};

	$scope.submitReport = function () {
		var data = encodeURIComponent(angular.toJson($scope.selectedReport));
		$scope.selectedReport.working = true;
		VulnService.updateNessus5ReportSettingsJSON(data).then(function (d) {
			if (d.data.status == true) {
				$scope.selectedReport.working = false;
				$scope.selectedReport.updated = true;
				$timeout(function () { $scope.selectedReport.updated = false; $scope.selectedReport.editable = false; }, 5000);
			}
			else {
				$scope.selectedReport.working = true;
			}
		}, function (error) {
			alert('Error! Nessus5Reports.js');
		});
	};

	$scope.saveReportItem = function (reportItemData) {
		var data = encodeURIComponent(angular.toJson(reportItemData));
		reportItemData.working = true;
		VulnService.updateNessus5ReportDataItemSettingsJSON(data).then(function (d) {
			console.log(d.data.status);
			if (d.data.status == true) {
				reportItemData.working = false;
				reportItemData.updated = true;
				$timeout(function () { reportItemData.updated = false; reportItemData.editable = false; }, 5000);
			}
			else {
				reportItemData.working = true;
			}
		}, function (error) {
			alert('Error! Nessus5Reports.js');
		});
	};

	$scope.placement = {
		options: [
			'top',
			'top-left',
			'top-right',
			'bottom',
			'bottom-left',
			'bottom-right',
			'left',
			'left-top',
			'left-bottom',
			'right',
			'right-top',
			'right-bottom'
		],
		selected: 'right-bottom'
	};
	$scope.dynamicPopover = {
		Description: 'Description',
		templateUrl: 'Nessus5ReportItemDetails',
		Name: 'Title',
		PluginID: 'pluginID'
	};
	$scope.changeDynamicContent = function (data) {
		$scope.dynamicPopover.content = data;
	};
}])

app.factory('VulnService', function ($http) {
	var fac = {};
	fac.reportList = function () {
		return $http.get('/VulnManagement/Nessus5ReportsJSON')
	}
	fac.getNessus5ReportItemsJSON = function (reportGuid, severityGroup) {
		return $http.get('/VulnManagement/getNessus5ReportItemsJSON?reportGuid=' + reportGuid + '&severityGroup=' + severityGroup)
	}
	fac.updateNessus5ReportSettingsJSON = function (data) {
		return $http.get('/VulnManagement/updateNessus5ReportSettingsJSON?report=' + data)
	}
	fac.updateNessus5ReportDataItemSettingsJSON = function (data) {
		return $http.get('/VulnManagement/updateNessus5ReportDataItemSettingsJSON?report=' + data)
	}
	fac.NMSummary = function () {
		return $http.get('/VulnManagement/NMSummary')
	}

	return fac;
});



app.factory('VMSummaryService', function ($http) {
	var fac = {};

	fac.NMSummary = function () {
		return $http.get('/VulnManagement/WW_SummaryJSON')
	}
	fac.getSummaryServerList = function () {
		return $http.get('/VulnManagement/WW_getSummaryServerList')
	}
	fac.getSummaryAppList = function () {
		return $http.get('/VulnManagement/WW_getSummaryAppList')
	}
	return fac;
});


app.directive('tooltip', function () {
	return {
		restrict: 'A',
		link: function (scope, element, attrs) {
			$(element).hover(function () {
				// on mouseenter
				$(element).tooltip('show');
			}, function () {
				// on mouseleave
				$(element).tooltip('hide');
			});
		}
	};
});

app.factory('f5Service', function ($http) {
	var fac = {};
	fac.GetJobs = function () {
		return $http.get('/F5Data/GetJobs')
	}

	fac.f5Summary = function () {
		return $http.get('/F5Data/f5Summary')
	}
	fac.EmailCertData = function () {
		return $http.get('/F5Data/EmailCertData')
	}


	fac.deletejobsbyguids = function (guids) {
		return $http.get('/F5Data/DeleteJobsIndividuallyJSON?guids=' + guids)
	}
	fac.deletejobsbyDate = function (date) {
		return $http.get('/F5Data/DeleteJobsByDate?dateString=' + date)
	}

	return fac;
});

app.controller('modalCtrl', ['$uibModalInstance', '$scope', '$log', 'option',
	function ($ubiModalInstance, $scope, $log, option) {
		$log.info(option);
		$scope.option = option;
		$scope.close = function () {
			$ubiModalInstance.close();
		}
	}
]);

app.run(['$anchorScroll', function ($anchorScroll) {
	$anchorScroll.yOffset = 50;   // always scroll by 50 extra pixels
}])


app.controller('IPNetworkController', ['NetService', '$scope', '$compile', '$log', '$interval', '$uibModal', 'uiGridConstants', '$timeout', function (NetService, $scope, $compile, $log, $interval, $uibModal, uiGridConstants, $timeout) {
	$scope.Network = [];
	$scope.added = false;

	$scope.TriggerAngular7 = function () {
		var html = AngularTemplateBody;
		var compiledHtml = $compile(html)($scope);
		angular.element(document.getElementById('AngularTemplatePlaceHolder002')).append(compiledHtml);

		NetService.GetNetworks().then(function (d) {
			$scope.gridOptions.data = d.data;
		}, function (error) {
			alert('Error! IPNetworksAdmin.js');
		});
	}



	$scope.highlightFilteredHeader = function (row, rowRenderIndex, col, colRenderIndex) {
		if (col.filters[0].term) {
			return 'header-filtered';
		} else {
			return '';
		}
	};
	$scope.getSelectedRows = function () {
		$scope.Network = $scope.gridApi.selection.getSelectedRows();
		console.log($scope.Network[0]);
	};
	$scope.gridOptions = {
		enableFiltering: true,
		showGridFooter: true,
		enableColumnResizing: true,
		minRowsToShow: "10",
		enableRowSelection: true,
		enableRowHeaderSelection: true,
		enableGridMenu: true,
		rowTemplate: '<div ng-class="{\'grey\':row.entity.Color==\'grey\', \'orange\':row.entity.Color==\'orange\', \'blue\':row.entity.Color==\'blue\', \'red\':row.entity.Color==\'red\', \'yellow\':row.entity.Color==\'yellow\' }"><div ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.colDef.name" class="ui-grid-cell" ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader }" ui-grid-cell></div></div>',
		enableSelectAll: true,
		exporterCsvFilename: 'myFile.csv',
		exporterPdfDefaultStyle: { fontSize: 9 },
		exporterPdfTableStyle: { margin: [30, 30, 30, 30] },
		exporterPdfTableHeaderStyle: { fontSize: 10, bold: true, italics: true, color: 'red' },
		exporterPdfHeader: { text: "HP CMS Networks", style: 'headerStyle' },
		exporterPdfFooter: function (currentPage, pageCount) {
			return { text: currentPage.toString() + ' of ' + pageCount.toString(), style: 'footerStyle' };
		},
		exporterPdfCustomFormatter: function (docDefinition) {
			docDefinition.styles.headerStyle = { fontSize: 22, bold: true };
			docDefinition.styles.footerStyle = { fontSize: 10, bold: true };
			return docDefinition;
		},
		exporterPdfOrientation: 'landscape',
		exporterPdfPageSize: 'LETTER',
		exporterPdfMaxGridWidth: 500,
		exporterCsvLinkElement: angular.element(document.querySelectorAll(".custom-csv-link-location")),
		onRegisterApi: function (gridApi) {
			$scope.gridApi = gridApi;
		}

	};
	$scope.gridOptions.multiSelect = false;
	$scope.gridOptions.modifierKeysToMultiSelect = false;
	$scope.gridOptions.noUnselect = true;
	$scope.gridOptions.selectedRow = $scope.Network;
	$scope.gridOptions.afterSelectionChange = function (data) {
		console.log($scope.Network[0])
	};
	$scope.gridOptions.onRegisterApi = function (gridApi) {
		$scope.gridApi = gridApi;
		$scope.gridApi.selection.on.rowSelectionChanged($scope, $scope.getSelectedRows);

	};

	$scope.gridOptions.columnDefs = [
		// default
		{
			field: 'Site',
			filter: {
				term: 'CDC',
				type: uiGridConstants.filter.SELECT,
				selectOptions: [{ value: 'CDC', label: 'CDC' }, { value: 'LDC', label: 'LDC' }, { value: 'CUL', label: 'CUL' }]
			},
			cellFilter: 'mapSite',
			headerCellClass: $scope.highlightFilteredHeader,
			width: "70"
		},
		{
			field: 'ApplicationEnvironment',
			filter: {
				term: '',
				type: uiGridConstants.filter.SELECT,
				selectOptions: [
					{ value: '3P Dev', label: '3P Dev' },
					{ value: '3P Impl', label: '3P Impl' },
					{ value: '3P Production', label: '3P Production' },
					{ value: 'Admin VLANs', label: 'Admin VLANs' },
					{ value: 'AWS (Amazon Web Services)', label: 'AWS (Amazon Web Services)' },
					{ value: 'CMS_AD', label: 'CMS_AD' },
					{ value: 'CMSNet VRFs', label: 'CMSNet VRFs' },
					{ value: 'Dedicated DEV/TEST Mgt Tools', label: 'Dedicated DEV/TEST Mgt Tools' },
					{ value: 'Dedicated IMP Mgt Tools', label: 'Dedicated IMP Mgt Tools' },
					{ value: 'Dedicated PROD Mgt Tools', label: 'Dedicated PROD Mgt Tools' },
					{ value: 'EIDM Development', label: 'EIDM Development' },
					{ value: 'EIDM IMP0', label: 'EIDM IMP0' },
					{ value: 'EIDM IMP1', label: 'EIDM IMP1' },
					{ value: 'EIDM Production', label: 'EIDM Production' },
					{ value: 'EIDM TEST0', label: 'EIDM TEST0' },
					{ value: 'EIDM TEST1', label: 'EIDM TEST1' },
					{ value: 'EXTERNAL NETWORKS', label: 'EXTERNAL NETWORKS' },
					{ value: 'HDI (Hosted Desktop Infrastructure)', label: 'HDI (Hosted Desktop Infrastructure)' },
					{ value: 'HIM IMP  ', label: 'HIM IMP  ' },
					{ value: 'HIM Production', label: 'HIM Production' },
					{ value: 'HIM T&D Development  Production', label: 'HIM T&D Development  Production' },
					{ value: 'MainFrame', label: 'MainFrame' },
					{ value: 'Management', label: 'Management' },
					{ value: 'Misc VLANs & IP Space', label: 'Misc VLANs & IP Space' },
					{ value: 'NAT', label: 'NAT' },
					{ value: 'Test & Dev Test0 Production', label: 'Test & Dev Test0 Production' },
					{ value: 'Test & Dev Test1 Production', label: 'Test & Dev Test1 Production' },
					{ value: 'Unix PREP', label: 'Unix PREP' },
					{ value: 'Unix Production', label: 'Unix Production' },
					{ value: 'VPN Client', label: 'VPN Client' },
					{ value: 'VPN SSL', label: 'VPN SSL' },
					{ value: 'Web Hosting Development', label: 'Web Hosting Development' },
					{ value: 'Web Hosting Implementation', label: 'Web Hosting Implementation' },
					{ value: 'Web Hosting Production', label: 'Web Hosting Production' },
					{ value: 'Web Hosting Test', label: 'Web Hosting Test' },
					{ value: 'Windows PREP', label: 'Windows PREP' },
					{ value: 'Windows Production', label: 'Windows Production' },
					{ value: 'Zone 0', label: 'Zone 0' }
				]
			},
			headerCellClass: $scope.highlightFilteredHeader,
			width: "150"
		},
		{
			field: 'Subnet', headerCellClass: $scope.highlightFilteredHeader, width: "*", resizable: true,


		},
		{ field: 'IPv4DefaultGateway', displayName: 'IPv4 DGW', headerCellClass: $scope.highlightFilteredHeader, width: "*", resizable: true },
		{ field: 'MASK', headerCellClass: $scope.highlightFilteredHeader, width: "*", resizable: true },
		{ field: 'IPv6', displayName: ' IPv6', headerCellClass: $scope.highlightFilteredHeader, width: "*", resizable: true },
		{ field: 'IPv6DefaultGateway', displayName: 'IPv6 DGW', headerCellClass: $scope.highlightFilteredHeader, width: "*", resizable: true },
		{ field: 'VLAN', headerCellClass: $scope.highlightFilteredHeader, width: "60", resizable: true },
		{ field: 'ASSIGNMENT', headerCellClass: $scope.highlightFilteredHeader, width: "*", resizable: false },
		{ field: 'VRF', headerCellClass: $scope.highlightFilteredHeader, width: "*", resizable: true },
		{ field: 'NetworkDevice', headerCellClass: $scope.highlightFilteredHeader, width: "*", resizable: true },
		{ field: 'MonitoringDevice', headerCellClass: $scope.highlightFilteredHeader, width: "150", resizable: true },
		{ field: 'Color', headerCellClass: $scope.highlightFilteredHeader, width: "*", resizable: true },

	];

	$scope.toggleFiltering = function () {
		$scope.gridOptions.enableFiltering = !$scope.gridOptions.enableFiltering;
		$scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
	};
	$scope.newNetwork = function () {
		$scope.Network = [];
	};

	$scope.saveNetwork = function () {
		$scope.gridOptions.data = [];
		$scope.added = false;
		console.log($scope.Network[0]);
		delete $scope.Network[0].$$hashKey;
		var data = JSON.stringify($scope.Network[0]);

		NetService.saveNetworks(data).then(function (d) {
			$scope.gridOptions.data = d.data;
			$scope.Network = [];
			$scope.added = true;
			$scope.gridApi.grid.refresh()
			$timeout(function () { $scope.added = false; }, 10000);
		}, function (error) {
			alert('Error! IPNetworksAdmin.js');
		});
	};

	$scope.deleteNetwork = function () {
		$scope.gridOptions.data = [];
		$scope.added = false;

		NetService.deleteNetworks($scope.Network[0].guid).then(function (d) {
			$scope.gridOptions.data = d.data;
			$scope.Network = [];
			$scope.added = true;
			$timeout(function () { $scope.added = false; }, 10000);
		}, function (error) {
			alert('Error! IPNetworksAdmin.js');
		});
	};
}])

app.filter('mapSite', function () {
	var siteHash = {
		CDC: 'CDC',
		CUL: 'CUL',
		LDC: 'LDC'
	};
	return function (input) {
		if (!input) {
			return '';
		} else {
			return siteHash[input];
		}
	};
});

app.factory('NetService', function ($http) { // explained about factory in Part2
	var fac = {};
	fac.GetNetworks = function () {
		return $http.get('/SwitchPorts/IPNetworksData')
	}
	fac.saveNetworks = function (network) {
		return $http.get('/SwitchPorts/addNetwork?network=' + network)
	}
	fac.deleteNetworks = function (guid) {
		return $http.get('/SwitchPorts/deleteNetwork?id=' + guid)
	}
	return fac;
});


app.controller('VMAppController', ['VMAppService', '$http', '$scope', '$compile', '$log', '$interval', '$uibModal', '$timeout',
	function (VMAppService, $http, $scope, $compile, $log, $interval, $uibModal, $timeout) {
		$scope.loading = false;
		$scope.serverList = [];
		$scope.Notifications = [];
		$scope.selectedServer = "All";
		var _selected;
		$scope.currentPage = 1;
		$scope.recordList = [];
		$scope.tracks = [];
		$scope.recordDetails = "";
		$scope.recordOwner = "All";
		$scope.recordCount = 0;
		$scope.maxSize = 20;
		$scope.filterOn = false;
		$scope.serverCount = 0;
		$scope.actionCount = 0;
		$scope.remediatedCount = 0;
		$scope.selectedOwner = 'All';
		$scope.remediated = false;
		$scope.massUpdate = false;
		$scope.massUpdateShowConfirm = false;
		$scope.massUpdateRecordCount = 0;
		//$scope.sharedData = $cookies.get('sendApp');
		$scope.assignedTo = "";

		if ($scope.sharedData != null) {
			$scope.selectedApp = $scope.sharedData;
			//$cookies.remove('sendApp');
			$scope.loading = true;
			VMAppService.GetServers($scope.selectedApp).then(function (d) {
				$scope.serverList = d.data.serverList;
				$scope.getOwnerList();
				$scope.loading = false;
				$scope.SMGroups = d.data.SMGroups;
			}, function (error) {
				alert('Error! VM-WW-ManageApp.js');
			});

		};


		$scope.TriggerAngular005 = function () {
			var html = AngularTemplateBody;
			var compiledHtml = $compile(html)($scope);
			angular.element(document.getElementById('AngularTemplatePlaceHolder005')).append(compiledHtml);

			VMAppService.NMSummary().then(function (d) {
				$scope.appList = d.data.appList;
				$scope.SMGroups = d.data.SMGroups;
				console.log("get App list");
				console.log(d.data);
			}, function (error) {
				alert('Error! VM-WW-ManageApp.js');
			});
		}

		$scope.updateServerList = function () {
			$scope.loading = true;
			VMAppService.GetServers($scope.selectedApp).then(function (d) {
				$scope.serverList = d.data.serverList;
				console.log("get server List");
				console.log(d.data);
				$scope.getOwnerList();
				$scope.loading = false;

			}, function (error) {
				alert('Error! VM-WW-ManageApp.js');
			});
		};

		$scope.getOwnerList = function () {
			$scope.loading = true;
			VMAppService.GetOwners($scope.selectedApp, $scope.selectedServer).then(function (d) {
				$scope.ownersList = d.data.ownersList;
				$scope.updateStats();
				console.log("get owners list");
				console.log(d.data);
				$scope.loading = false;

			}, function (error) {
				alert('Error! VM-WW-ManageApp.js');
			});
		};

		$scope.pullData = function () {
			$scope.loading = true;

			return $http.get('/VulnManagement/WW_AppGetRecordsJSON', {
				params: {
					app: $scope.selectedApp,
					server: $scope.selectedServer,
					owner: $scope.recordOwner,
					page: $scope.currentPage,

				}
			}).then(function (d) {
				console.log(d.data);
				$scope.recordList = d.data.recordList;
				$scope.totalItems = d.data.itemCount;
				$scope.loading = false;
			});
		};

		$scope.updateStats = function () {
			$scope.loading = true;
			VMAppService.UpdateStats($scope.selectedApp, $scope.selectedServer, $scope.recordOwner).then(function (d) {
				$scope.recordCount = d.data.count;
				console.log("get recordCount");
				console.log(d.data);
				$scope.loading = false;

			}, function (error) {
				alert('Error! VM-WW-ManageApp.js');
			});
		};

		$scope.pageChanged = function () {
			$scope.pullData();
		};

		$scope.changeOwner = function () {

			$scope.currentPage = 1;
			$scope.getData();
		};

		$scope.dynamicPopover = {
			Description: 'Description',
			templateUrl: 'VulnManagement/myPopoverTemplate',
			Name: 'Title',
			PluginID: 'pluginID'
		};

		$scope.submitUpdate = function (sourceRecord) {
			console.log(sourceRecord);
			sourceRecord.editable = true;
			sourceRecord.working = true;

			VMAppService.updateRecord(sourceRecord.guid, sourceRecord.remediated, sourceRecord.businessApp, sourceRecord.assignedTo, sourceRecord.programApp, sourceRecord.notes).then(function (d) {
				console.log(d);
				if (d.data.success == 'true') {
					sourceRecord.working = false;
					sourceRecord.updated = true;
					$timeout(function () { sourceRecord.updated = false; sourceRecord.editable = false; }, 5000);
				}
			}, function (error) {
				alert('Error! VM-WW-ManageApp.js');
			});

		};

		$scope.ConfirmMassUpdate = function (environ, server, owner, detailsFilter) {
			VMAppService.ConfirmMassUpdate(environ, server, owner, detailsFilter).then(function (d) {
				console.log(environ + owner + detailsFilter)
				$scope.massUpdateRecordCount = d.data.count;
				$scope.massUpdateShowConfirm = true;
			}, function (error) {
				alert('Error! VM-WW-ManageApp.js');
			});

		};

		$scope.ApplyMassUpdate = function (environ, server, owner, detailsFilter, remediated, busApp, appOwner, programApp, notes) {
			VMAppService.ApplyMassUpdate(environ, server, owner, detailsFilter, remediated, busApp, appOwner, programApp, notes).then(function (d) {
				console.log("apply Mass Update")
				if (d.data.success == 'success') {
					$scope.massUpdateResults = true;
					$scope.pullData();
					$scope.massUpdateShowConfirm = false;
				}
			}, function (error) {
				alert('Error! VM-WW-ManageApp.js');
			});

		};
	}])

app.factory('VMAppService', function ($http) {
	var fac = {};
	fac.NMSummary = function () {
		return $http.get('/VulnManagement/WW_ManageDataJSON')
	}
	fac.GetServers = function (data) {
		return $http.get('/VulnManagement/WW_AppServerListJSON?app=' + data)
	}
	fac.GetOwners = function (app, server) {
		return $http.get('/VulnManagement/WW_AppOwnersListJSON?app=' + app + '&server=' + server)
	}
	fac.UpdateStats = function (app, server, owner) {
		return $http.get('/VulnManagement/WW_AppUpdateStatsJSON?app=' + app + '&server=' + server + '&owner=' + owner)
	}
	fac.GetRecords = function (app, server, owner) {
		return $http.get('/VulnManagement/WW_AppGetRecordsJSON?app=' + app + '&server=' + server + '&owner=' + owner)
	}
	fac.updateRecord = function (g, remediated, busApp, appOwner, programApp, notes) {
		return $http.get('/VulnManagement/WW_UpdateRecordData?g=' + g + '&remediated=' + remediated + '&busApp=' + busApp + '&appOwner=' + appOwner + '&programApp=' + programApp + '&notes=' + notes)
	}
	fac.ConfirmMassUpdate = function (environ, server, owner, detailsFilter) {
		return $http.get('/VulnManagement/WW_AppConfirmMassUpdate?environ=' + environ + '&server=' + server + '&owner=' + owner + '&detailsFilter=' + detailsFilter)
	}
	fac.ApplyMassUpdate = function (environ, server, owner, detailsFilter, remediated, busApp, appOwner, programApp, notes) {
		return $http.get('/VulnManagement/WW_AppApplyMassUpdate?environ=' + environ + '&server=' + server + '&owner=' + owner + '&detailsFilter=' + detailsFilter + '&remediated=' + remediated + '&busApp=' + busApp + '&appOwner=' + appOwner + '&programApp=' + programApp + '&notes=' + notes)
	}
	return fac;
});

app.controller('VMSearchController', ['VMSearchService', '$http', '$scope', '$compile', '$log', '$interval', '$uibModal', '$timeout',
	function (VMSearchService, $http, $scope, $compile, $log, $interval, $uibModal, $timeout) {
		$scope.loading = false;
		$scope.serverList = [];
		$scope.dynamicPopover = [];
		$scope.Notifications = [];
		$scope.selectedServer = "";
		$scope.recordList = [];
		var _selected;
		$scope.currentPage = 1;
		$scope.tracks = [];
		$scope.recordDetails = "";
		$scope.recordOwner = "";
		$scope.totalItems = 0;
		$scope.maxSize = 30;
		$scope.filterOn = false;
		$scope.serverCount = 0;
		$scope.actionCount = 0;
		$scope.remediatedCount = 0;
		$scope.remediated = false;
		$scope.massUpdate = false;
		$scope.massUpdateShowConfirm = false;
		$scope.massUpdateRecordCount = 0;
		//$scope.sharedData = $cookies.get('sendServer');
		$scope.assignedTo = "";
		$scope.searchSever = "";
		$scope.searchFile = "";
		$scope.searchActive = false;
		$scope.searchRemdiated = false;
		$scope.searchTag = "";
		$scope.searchOwner = "";
		$scope.programName = "";


		$scope.TriggerAngular006 = function () {
			var html = AngularTemplateBody;
			var compiledHtml = $compile(html)($scope);
			angular.element(document.getElementById('AngularTemplatePlaceHolder006')).append(compiledHtml);
		}


		$scope.findSearch = function () {
			$scope.loading = true;
			console.log("getting data");
			return $http.get('/VulnManagement/WW_ManageDataSearchJSON', {
				params: {
					server: $scope.searchServer,
					page: $scope.currentPage,
					owner: $scope.searchOwner,
					tag: $scope.searchTag,
					file: $scope.searchFile,
					active: $scope.searchActive,

				}
			}).then(function (response) {

				angular.copy(response.data.managementRecordList, $scope.recordList);
				$scope.totalItems = response.data.count;
				$scope.recordOwnerList = response.data.recordOwnerList;
				$scope.loading = false;
			});
		};

		$scope.searchConfirmMassUpdate = function () {
			$scope.loading = true;
			VMSearchService.searchConfirmMassUpdate($scope.searchServer, $scope.searchOwner, $scope.searchTag, $scope.searchFile, $scope.searchActive).then(function (d) {

				$scope.massUpdateRecordCount = d.data.count;
				$scope.searchmassUpdateShowConfirm = true;
				$scope.loading = false;
			}, function (error) {
				alert('Error! VM-WW-ManageSearch.js');
			});

		};

		//server, owner, tag, detailsFilter, remediated, programName, appName, appOwner, notes
		$scope.searchApplyMassUpdate = function () {
			$scope.loading = true;
			VMSearchService.searchApplyMassUpdate($scope.searchServer, $scope.searchOwner, $scope.searchTag, $scope.searchFile, $scope.searchActive, $scope.massRemediated, $scope.massUpdateBusinessApp, $scope.massUpdateAppName, $scope.massUpdateAppOwner, $scope.massUpdateNotes).then(function (d) {
				console.log("apply Mass Update")
				if (d.data.success == 'success') {
					$scope.massUpdateResults = true;
					$scope.findSearch();
					$scope.searchmassUpdateShowConfirm = false;
					$scope.loading = false;
				}
			}, function (error) {
				alert('Error! VM-WW-ManageSearch.js');
			});

		};

		$scope.getData = function () {
			$scope.loading = true;
			console.log("getting data");
			return $http.get('/VulnManagement/WW_GetServerRecordJSON', {
				params: {
					server: $scope.selectedServer,
					page: $scope.currentPage,
					recordOwner: $scope.recordOwner,
					recordDetails: $scope.recordDetails,
					remediated: $scope.remediated
				}
			}).then(function (response) {

				angular.copy(response.data.recordList, $scope.recordList);
				$scope.totalItems = response.data.itemCount;
				$scope.loading = false;
			});
		};

		$scope.getServer = function (val) {
			return $http.get('/VulnManagement/WW_GetServerListJSON', {
				params: {
					server: val,
				}
			}).then(function (response) {
				return response.data.serverList
			});
		};

		//get another portions of data on page changed
		$scope.pageChanged = function () {
			$scope.findSearch();
		};

		$scope.changeOwner = function () {
			$scope.currentPage = 1;
			$scope.getData();
		};

		$scope.dynamicPopover = {
			Description: 'Description',
			templateUrl: 'VulnManagement/myPopoverTemplate',
			Name: 'Title',
			PluginID: 'pluginID'
		};

		$scope.submitUpdate = function (sourceRecord) {
			console.log(sourceRecord);
			sourceRecord.editable = true;
			sourceRecord.working = true;

			VMSearchService.updateRecord(sourceRecord.guid, sourceRecord.remediated, sourceRecord.dwTicket, sourceRecord.assignedTo, sourceRecord.notes).then(function (d) {
				console.log(d);
				if (d.data.success == 'true') {
					sourceRecord.working = false;
					sourceRecord.updated = true;
					$timeout(function () { sourceRecord.updated = false; sourceRecord.editable = false; }, 5000);
				}
			}, function (error) {
				alert('Error! VM-WW-ManageSearch.js');
			});

		};

	}]);

app.factory('VMSearchService', function ($http) {
	var fac = {};
	fac.RecordList = function (data) {
		return $http.get('/VulnManagement/WW_RecordListJSON?server=' + data)
	}
	fac.updateRecord = function (g, remediated, appName, appOwner, notes) {
		return $http.get('/VulnManagement/WW_UpdateRecordData?g=' + g + '&remediated=' + remediated + '&appName=' + appName + '&appOwner=' + appOwner + '&notes=' + notes)
	}
	fac.searchConfirmMassUpdate = function (searchServer, searchOwner, searchTag, searchFile, searchActive) {
		return $http.get('/VulnManagement/WW_searchConfirmMassUpdate?server=' + searchServer + '&owner=' + searchOwner + '&tag=' + searchTag + '&file=' + searchFile + '&active=' + searchActive)
	}
	fac.searchApplyMassUpdate = function (server, owner, tag, file, searchActive, remediated, busName, appName, appOwner, notes) {
		return $http.get('/VulnManagement/WW_searchApplyMassUpdate?server=' + server + '&owner=' + owner + '&tag=' + tag + '&file=' + file + '&active=' + searchActive + '&remediated=' + remediated + '&busName=' + busName + '&programName=' + appName + '&appOwner=' + appOwner + '&notes=' + notes)
	}
	return fac;
});


app.controller('VMIndexController', ['VulnService', '$scope', '$compile', '$sce',
	function (VulnService, $scope, $compile, $sce) {
		$scope.loading = false;
		
		$scope.dynamicPopover = [];
	
		$scope.TriggerAngular007 = function () {
			var html = AngularTemplateBody;
			var compiledHtml = $compile(html)($scope);
			angular.element(document.getElementById('AngularTemplatePlaceHolder007')).append(compiledHtml);


			VulnService.NMSummary().then(function (d) {
				$scope.summaryData = [];
				$scope.Notifications = [];
				$scope.summaryData = d.data.summaryData;
				$scope.Notifications = d.data.criticalNotificationList;
				$scope.loading = false;
			},
				function (error) {
					alert('Error getting summary information!');
				});
		}

		$scope.placement = {
			options: [
				'top',
				'top-left',
				'top-right',
				'bottom',
				'bottom-left',
				'bottom-right',
				'left',
				'left-top',
				'left-bottom',
				'right',
				'right-top',
				'right-bottom'
			],
			selected: 'right-bottom'
		};

		$scope.dynamicPopover = {
			Description: 'Description',
			templateUrl: 'myPopoverTemplate',
			Name: 'Title',
			PluginID: 'pluginID'
		};

		$scope.changeDynamicContent = function (data) {
			$scope.dynamicPopover.Description = data.Description;
			$scope.dynamicPopover.Name = data.PluginName;
			$scope.dynamicPopover.PluginID = data.PluginID;
		}

	}])


app.controller('VMWaiverController', ['VMWaiverService', '$scope', '$compile', '$sce', '$log', '$interval', '$timeout',
	function (VMWaiverService, $scope, $compile, $sce, $log, $interval, $timeout) {
		$scope.loading = false;
		
		$scope.showDetails = false;
		$scope.EditWaiver = {};
		$scope.showCount = false;


		$scope.TriggerAngular008 = function () {
			var html = AngularTemplateBody;
			var compiledHtml = $compile(html)($scope);
			angular.element(document.getElementById('AngularTemplatePlaceHolder008')).append(compiledHtml);

			VMWaiverService.NMSummary().then(function (d) {
				$scope.waiverList = {};
				$scope.loading = true;
				$scope.waiverList = d.data.waiverList;
				$scope.loading = false;
			},
				function (error) {
					alert('Error getting summary information!');
				});

		}


		$scope.showDetailWaiver = function (d) {
			$scope.showDetails = true;
			$scope.EditWaiver = d;

		};

		$scope.createWaiver = function () {
			$scope.showDetails = true;
			$scope.EditWaiver = null;
			$scope.EditWaiver = {};
		};

		$scope.deleteWaiver = function (g) {
			$scope.loading = true;

			VMWaiverService.NMDeleteWaiver(g).then(function (d) {
				$scope.waiverList = d.data.waiverList;
				$scope.EditWaiver = {};
				$scope.showDetails = false;
				$scope.loading = false;

			}, function (error) {
				alert('Error! VM-WW-Waiver.js');
			});

		};

		$scope.submitWaiverChanges = function (d) {
			$scope.loading = true;
			var data = JSON.stringify(d);
			console.log(data);
			VMWaiverService.NMUpdateWaiver(data).then(function (d) {
				$scope.waiverList = d.data.waiverList;
				$scope.EditWaiver = {};
				$scope.showDetails = false;
				$scope.loading = false;

			}, function (error) {
				alert('Error! VM-WW-Waiver.js');
			});
		};

		$scope.applyWaiver = function (id) {
			$scope.loading = true;
			VMWaiverService.NMApplyWaiver(id).then(function (d) {
				console.log(d);
				$scope.waiverList = d.data.waiverList;
				$scope.showCount = true;
				$scope.count = d.data.count;
				$scope.EditWaiver = {};
				$scope.showDetails = false;
				$scope.loading = false;

			}, function (error) {
				alert('Error! VM-WW-Waiver.js');
			});
		};

		$scope.placement = {
			options: [
				'top',
				'top-left',
				'top-right',
				'bottom',
				'bottom-left',
				'bottom-right',
				'left',
				'left-top',
				'left-bottom',
				'right',
				'right-top',
				'right-bottom'
			],
			selected: 'top'
		};

		$scope.dynamicPopover = {
			Description: 'Description',
			templateUrl: 'VulnManagement/myPopoverTemplate',
			Name: 'Title',
			PluginID: 'pluginID'
		};

		$scope.changeDynamicContent = function (data) {
			$scope.dynamicPopover.Description = data.Description;
			$scope.dynamicPopover.Name = data.PluginName;
			$scope.dynamicPopover.PluginID = data.PluginID;
		}

		$scope.opened = {};

		$scope.open = function ($event, dt) {
			$event.preventDefault();
			$event.stopPropagation();

			dt.opened = true;
		};

		$scope.open2 = function ($event, dt) {
			$event.preventDefault();
			$event.stopPropagation();

			dt.opened2 = true;
		};


		$scope.today = function () {
			$scope.dt = new Date();
		};

		$scope.today();

		$scope.clear = function () {
			$scope.dt = null;
		};

		$scope.inlineOptions = {
			customClass: getDayClass,
			minDate: new Date(),
			showWeeks: true
		};

		$scope.dateOptions = {
			dateDisabled: disabled,
			formatYear: 'yy',
			maxDate: new Date(2020, 5, 22),
			minDate: new Date(),
			startingDay: 1
		};

		// Disable weekend selection
		function disabled(data) {
			//var date = data.date,
			//  mode = data.mode;
			//return mode === 'day' && (date.getDay() === 0 || date.getDay() === 6);
		}

		$scope.toggleMin = function () {
			$scope.inlineOptions.minDate = $scope.inlineOptions.minDate ? null : new Date();
			$scope.dateOptions.minDate = $scope.inlineOptions.minDate;
		};

		$scope.toggleMin();



		$scope.setDate = function (year, month, day) {
			$scope.dt = new Date(year, month, day);
		};

		$scope.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
		$scope.format = $scope.formats[0];
		$scope.altInputFormats = ['M!/d!/yyyy'];

		$scope.popup1 = {
			opened: false
		};

		$scope.popup2 = {
			opened: false
		};

		var tomorrow = new Date();
		tomorrow.setDate(tomorrow.getDate() + 1);
		var afterTomorrow = new Date();
		afterTomorrow.setDate(tomorrow.getDate() + 1);
		$scope.events = [
			{
				date: tomorrow,
				status: 'full'
			},
			{
				date: afterTomorrow,
				status: 'partially'
			}
		];

		function getDayClass(data) {
			var date = data.date,
				mode = data.mode;
			if (mode === 'day') {
				var dayToCheck = new Date(date).setHours(0, 0, 0, 0);

				for (var i = 0; i < $scope.events.length; i++) {
					var currentDay = new Date($scope.events[i].date).setHours(0, 0, 0, 0);

					if (dayToCheck === currentDay) {
						return $scope.events[i].status;
					}
				}
			}

			return '';
		};
	}])

app.factory('VMWaiverService', function ($http) {
	var fac = {};
	fac.NMSummary = function () {
		return $http.get('/VulnManagement/WW_WaiverStartJSON')
	}
	fac.NMUpdateWaiver = function (waiver) {
		return $http.get('/VulnManagement/WW_UpdateWaiverJSON?waiver=' + waiver)
	}
	fac.NMDeleteWaiver = function (g) {
		return $http.get('/VulnManagement/WW_DeleteWaiverJSON?g=' + g)
	}
	fac.NMApplyWaiver = function (id) {
		return $http.get('/VulnManagement/WW_ApplyWaiverJSON?waiverID=' + id)
	}
	return fac;
});

app.controller('VMServerController', ['VMServerService', '$http', '$scope', '$compile', '$log', '$interval', '$uibModal', '$timeout',
	function (VMServerService,  $http, $scope, $compile, $log, $interval, $uibModal, $timeout) {
		$scope.loading = false;
		$scope.serverList = [];
		$scope.dynamicPopover = [];
		$scope.Notifications = [];
		$scope.selectedServer = "";
		$scope.recordList = [];
		var _selected;
		$scope.currentPage = 1;
		$scope.tracks = [];
		$scope.recordDetails = "";
		$scope.recordOwner = "";
		$scope.totalItems = 0;
		$scope.maxSize = 20;
		$scope.filterOn = false;
		$scope.serverCount = 0;
		$scope.actionCount = 0;
		$scope.remediatedCount = 0;
		$scope.remediated = false;
		$scope.massUpdate = false;
		$scope.massUpdateShowConfirm = false;
		$scope.massUpdateRecordCount = 0;
		//$scope.sharedData = $cookies.get('sendServer');
		$scope.assignedTo = "";
		$scope.massRemediated = false;
		$scope.massUpdateBusinessApp = "";
		$scope.massUpdateAppOwner = "";
		$scope.massUpdateProgramApp = "";
		$scope.massUpdateNotes = "";


		$scope.TriggerAngular009 = function () {
			var html = AngularTemplateBody;
			var compiledHtml = $compile(html)($scope);
			angular.element(document.getElementById('AngularTemplatePlaceHolder009')).append(compiledHtml);

		}



		if ($scope.sharedData != null) {
			$scope.selectedServer = $scope.sharedData;
			//$cookies.remove('sendServer');
			$scope.afterSelectedServer;
			VMServerService.RecordList($scope.selectedServer).then(function (d) {
				$scope.getData();
				console.log(d.data)
				$scope.UCMDBInfo = d.data.UCMDBInfo;
				$scope.serverInfo = d.data.serverInfo;
				$scope.reportDate = d.data.reportDate;
				$scope.SMGroups = d.data.SMGroups;
				$scope.recordOwnerList = d.data.recordOwnerList;
				$scope.remediatedCount = d.data.remediatedCount;
				$scope.actionCount = d.data.actionCount;
				$scope.serverCount = d.data.serverCount;
				$scope.loading = false;
			});
		};

		$scope.afterSelectedServer = function () {
			$scope.recordList = [];
			$scope.loading = true;
			$scope.recordDetails = "";
			$scope.recordOwner = "";

			VMServerService.RecordList($scope.selectedServer).then(function (d) {
				$scope.getData();
				console.log(d.data)
				$scope.SMGroups = d.data.SMGroups;
				$scope.UCMDBInfo = d.data.UCMDBInfo;
				$scope.serverInfo = d.data.serverInfo;
				$scope.reportDate = d.data.reportDate;
				$scope.recordOwnerList = d.data.recordOwnerList;
				$scope.remediatedCount = d.data.remediatedCount;
				$scope.actionCount = d.data.actionCount;
				$scope.serverCount = d.data.serverCount;
				$scope.loading = false;
			});

		};

		$scope.getData = function () {
			$scope.loading = true;
			console.log("getting data");
			return $http.get('/VulnManagement/WW_GetServerRecordJSON', {
				params: {
					server: $scope.selectedServer,
					page: $scope.currentPage,
					recordOwner: $scope.recordOwner,
					recordDetails: $scope.recordDetails,
					remediated: $scope.remediated
				}
			}).then(function (response) {

				angular.copy(response.data.recordList, $scope.recordList);
				$scope.totalItems = response.data.itemCount;
				$scope.loading = false;
			});
		}
		$scope.getServer = function (val) {
			return $http.get('/VulnManagement/WW_GetServerListJSON', {
				params: {
					server: val,
				}
			}).then(function (response) {
				return response.data.serverList
			});
		};

		//get another portions of data on page changed
		$scope.pageChanged = function () {
			$scope.getData();
		};
		$scope.changeOwner = function () {
			$scope.currentPage = 1;
			$scope.getData();
		};
		$scope.dynamicPopover = {
			Description: 'Description',
			templateUrl: 'VulnManagement/myPopoverTemplate',
			Name: 'Title',
			PluginID: 'pluginID'
		};
		$scope.submitUpdate = function (sourceRecord) {
			console.log(sourceRecord);
			sourceRecord.editable = true;
			sourceRecord.working = true;

			VMServerService.updateRecord(sourceRecord.guid, sourceRecord.remediated, sourceRecord.dwTicket, sourceRecord.assignedTo, sourceRecord.notes).then(function (d) {
				console.log(d);
				if (d.data.success == 'true') {
					sourceRecord.working = false;
					sourceRecord.updated = true;
					$timeout(function () { sourceRecord.updated = false; sourceRecord.editable = false; }, 5000);
				}
			}, function (error) {
				alert('Error! VM-WW-Manage.js');
			});

		};
		$scope.ConfirmMassUpdate = function (server, owner, detailsFilter) {
			VMServerService.ConfirmMassUpdate(server, owner, detailsFilter).then(function (d) {
				console.log(server + owner + detailsFilter)
				$scope.massUpdateRecordCount = d.data.count;
				$scope.massUpdateShowConfirm = true;
			}, function (error) {
				alert('Error! VM-WW-Manage.js');
			});

		};
		$scope.ApplyMassUpdate = function () {
			console.log($scope.selectedServer + " " +
				$scope.recordOwner + " " +
				$scope.recordDetails + " " +
				$scope.massRemediated + " " +
				$scope.massUpdateBusinessApp + " " +
				$scope.massUpdateAppOwner + " " +
				$scope.massUpdateProgramApp + " " +
				$scope.massUpdateNotes);
			$scope.loading = true;
			VMServerService.ApplyMassUpdate(
				$scope.selectedServer,
				$scope.recordOwner,
				$scope.recordDetails,
				$scope.massRemediated,
				$scope.massUpdateBusinessApp,
				$scope.massUpdateAppOwner,
				$scope.massUpdateProgramApp,
				$scope.massUpdateNotes).then(function (d) {
					console.log("apply Mass Update")
					if (d.data.success == 'success') {
						$scope.massUpdateResults = true;
						$scope.getData();
						$scope.massUpdateShowConfirm = false;
						$scope.loading = false;
					}
				}, function (error) {
					alert('Error! VM-WW-Manage.js');
				});

		};

	}
])

app.factory('VMServerService', function ($http) {
	var fac = {};
	fac.RecordList = function (data) {
		return $http.get('/VulnManagement/WW_RecordListJSON?server=' + data)
	}
	fac.updateRecord = function (g, remediated, appName, appOwner, notes) {
		return $http.get('/VulnManagement/WW_UpdateRecordData?g=' + g + '&remediated=' + remediated + '&appName=' + appName + '&appOwner=' + appOwner + '&notes=' + notes)
	}
	fac.ConfirmMassUpdate = function (server, owner, detailsFilter) {
		return $http.get('/VulnManagement/WW_ConfirmMassUpdate?server=' + server + '&owner=' + owner + '&detailsFilter=' + detailsFilter)
	}
	fac.ApplyMassUpdate = function (server, owner, detailsFilter, remediated, busApp, appOwner, programApp, notes) {
		return $http.get('/VulnManagement/WW_ApplyMassUpdate?server=' + server + '&owner=' + owner + '&detailsFilter=' + detailsFilter + '&remediated=' + remediated + '&busApp=' + busApp + '&appOwner=' + appOwner + '&programApp=' + programApp + '&notes=' + notes)
	}
	return fac;
});