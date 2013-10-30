'use strict';

angular.module('colorpicker-ng', [])
  .directive('colorPicker', function () {
      var colors = ["0000ff",  // Blue
                    "ff0000",  // Red 
                    "007f00",  // Green 
                    "ffff00",  // Yellow
                    "ff7f00",  // Orange
                    "7f0000",  // Brown 
                    "7f007f",  // Purple
                    "ff7fff"]; // Pink
      return {
          scope: {
              color: '=colorPicker'
          },
          link: function(scope, element, attrs) {
              element.colorPicker({
                  // initialize the color to the color on the scope
                  pickerDefault: scope.color,
                  colors: colors,
                  showHexField: false,
                  // update the scope whenever we pick a new color
                  onColorChange: function(id, newValue) {
                      scope.$apply(function() {
                          scope.color = newValue;
                      });
                      var blurFunc = attrs.onBlur;
                      if (blurFunc !== undefined) {
                          scope.$parent.$eval(blurFunc);
                      }
                  }
              });

              // update the color picker whenever the value on the scope changes
              scope.$watch('color', function(value) {
                  element.val(value);
                  element.change();                
              });
          }
      }
  });
