# Parlo.Docker
<p align="center"> <img src="https://github.com/Afr0/Parlo/blob/main/Logo.png"/></p>
 A Docker container that demonstrates how to implement a <a href="https://github.com/secure-remote-password/srp.net">Secure Remote Password-based</a> login protocol with <a href="https://www.github.com/afr0/parlo/">Parlo</a>.

To build: cd into directory, then <b>docker build -t parlo .</b>

To run: cd into directory, then <b>docker run -d -p 3077:3077 -p 8080:80 parlo</b>

<i>This will spawn a website on localhost:8080 in your browser where you can register a new user.</i>
