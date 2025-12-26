# How to generate test data

There are a handful of scripts that generate the test data for these tests. There are also a handful of small tif files under version control that should be removed and moved to a script. 

We may have to drop down to bash to use GDAL commands because rasterio is quite difficult to do more complex stuff with (e.g. explicitly creating EXTERNAL overview files) 

