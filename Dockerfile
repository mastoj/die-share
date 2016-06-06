FROM fsharp/fsharp
EXPOSE 8033
ENV approot=/app \
    VIRTUAL_HOST=my-share.lol.com
WORKDIR ${approot}
ADD . $approot
CMD ["./build.sh","run"]
