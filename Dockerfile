FROM fsharp/fsharp
ENV approot /app
WORKDIR ${approot}
ADD . $approot
CMD ["./build.sh","run"]
