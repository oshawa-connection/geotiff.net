# How to setup:

Download s3Proxy [from here](https://github.com/gaul/s3proxy), MAKING SURE YOU DOWNLOAD AT LEAST V2.9.0 due to [this issue](https://github.com/gaul/s3proxy/issues/806)

Put the S3Proxy executable under RemoteTests/S3Proxy.
```shell
cd RemoteTests/S3Proxy
chmod +x s3proxy
s3proxy --properties s3proxy.conf
```