const CircleStyleDefault = "rounded-circle bg-secondary";
const CircleStyleBlue = "rounded-circle bg-primary";
const CircleStyleGreen = "rounded-circle bg-success";
const CircleStyleRed = "rounded-circle bg-danger";

const amnesiaTile = document.getElementById('AmnesiaTile');
const amnesiaTileCircle = document.getElementById('AmnesiaTileCircle');
const amnesiaTileDesc = document.getElementById('AmnesiaTileDesc');
const amnesiaTileActionsDiv = document.getElementById('AmnesiaTileActionsDiv');

const twitchTile = document.getElementById('TwitchTile');
const twitchTileCircle = document.getElementById('TwitchTileCircle');
const twitchTileDesc = document.getElementById('TwitchTileDesc');
const twitchTileActionsDiv = document.getElementById('TwitchTileActionsDiv');

const splashScreen = document.getElementById("SplashScreen");

const twitchChaosButton = document.getElementById("TwitchChaosBtn");
const localChaosButton = document.getElementById("LocalChaosBtn");

const chaosErrorLabel = document.getElementById("ChaosErrorLabel");

function closeSplash() {
    splashScreen.remove();
    localStorage.setItem('NO-SPLASH', true);
}

if (localStorage.getItem('NO-SPLASH')) {
    closeSplash();
}

function onAmnesiaConnectPressed(event) {
    amnesiaStartButton.disabled = true;
    startClient();
    event.preventDefault();
}

function onAmnesiaDisconnectPressed(event) {
    amnesiaStopButton.disabled = true;
    stopClient();
    event.preventDefault();
}

function onTwitchConnectPressed(event) {
    twitchStartButton.disabled = true;
    startTwitchBot();
    event.preventDefault();
}

function onTwitchDisconnectPressed(event) {
    console.log('disconnect pressed');
    twitchStopButton.disabled = true;
    stopTwitchBot();
    event.preventDefault();
}

function runCmdQueueTest() {
    connection.invoke("RunCommandQueueTest").catch(function (err) {
        return console.error(err.toString());
    });
}

function loadPayloadsTest() {
    connection.invoke("LoadPayloadsTest").catch(function (err) {
        return console.error(err.toString());
    });
}

function startLocalChaos() {
    localChaosButton.disabled = true;
    twitchChaosButton.disabled = true;

    connection.invoke("StartLocalChaos").catch(function (err) {
        return console.error(err.toString());
    });
}

function startTwitchPoll() {
    localChaosButton.disabled = true;
    twitchChaosButton.disabled = true;

    connection.invoke("StartTwitchPollChaos").catch(function (err) {
        return console.error(err.toString());
    });
}

function startTwitchBot() {
    connection.invoke("StartTwitchBot").catch(function (err) {
        return console.error(err.toString());
    });
}

function stopTwitchBot() {
    connection.invoke("StopTwitchBot").catch(function (err) {
        return console.error(err.toString());
    });
}

const amnesiaStartButton = document.createElement('button');
amnesiaStartButton.classList.add('btn', 'btn-outline-success', 'btn-sm', 'd-block');
amnesiaStartButton.innerText = 'Connect';
amnesiaStartButton.addEventListener('click', onAmnesiaConnectPressed);

const amnesiaStopButton = document.createElement('button');
amnesiaStopButton.classList.add('btn', 'btn-outline-danger', 'btn-sm', 'dblock');
amnesiaStopButton.innerText = 'Disconnect';
amnesiaStopButton.addEventListener('click', onAmnesiaDisconnectPressed);

const twitchStartButton = document.createElement('button');
twitchStartButton.classList.add('btn', 'btn-outline-success', 'btn-sm', 'd-block');
twitchStartButton.innerText = 'Connect';
twitchStartButton.addEventListener('click', onTwitchConnectPressed);

const twitchStopButton = document.createElement('button');
twitchStopButton.classList.add('btn', 'btn-outline-danger', 'btn-sm', 'd-block');
twitchStopButton.innerText = 'Disconnect';
twitchStopButton.addEventListener('click', onTwitchDisconnectPressed);

const amnesiaWarningText = document.createElement('small');
amnesiaWarningText.classList.add('text-warning');

const amnesiaErrorText = document.createElement('small');
amnesiaErrorText.classList.add('text-danger');

const twitchWarningText = document.createElement('small');
twitchWarningText.classList.add('text-warning');

const twitchErrorText = document.createElement('small');
twitchErrorText.classList.add('text-danger');

const connection = new signalR.HubConnectionBuilder().withUrl("/statushub").build();
connection.start().catch(function (err) {
    return console.error(err.toString());
});

// TODO: Block until SignalR connects
connection.on("AmnesiaClientStateChanged", setAmnesiaTileState);
connection.on("TwitchBotClientStateChanged", setTwitchTileState);
connection.on("OnTwitchBotError", setTwitchError);

connection.on("ChaosError", setChaosError);

let amnesiaState = "Disconnected";
let twitchState = "Disconnected";

function onAnyStateChanged() {
    if (amnesiaState !== "Connected") {
        connection.invoke("StopAllConductors").catch(function (err) {
            return console.error(err.toString());
        });

        localChaosButton.disabled = true;
        twitchChaosButton.disabled = true;
    }
    else {
        localChaosButton.disabled = false;

        if (twitchState === "Connected") {
            twitchChaosButton.disabled = false;
        }
        else {
            connection.invoke("StopAllConductors").catch(function (err) {
                return console.error(err.toString());
            });
            twitchChaosButton.disabled = true;
        }
    }
}

function startClient() {
    connection.invoke("StartAmnesiaClient").catch(function (err) {
        return console.error(err.toString());
    });
}

function stopClient() {
    connection.invoke("StopAmnesiaClient").catch(function (err) {
        return console.error(err.toString());
    });
}

function setAmnesiaTileState(state, message) {
    console.log("Amnesia client state: ", state);
    amnesiaState = state;
    onAnyStateChanged();

    if (state === 'Disconnected') {
        amnesiaTileCircle.className = CircleStyleDefault;
        amnesiaTileDesc.innerText = 'Not running';
        amnesiaStartButton.disabled = false;
        amnesiaWarningText.innerText = 'Open Amnesia.exe before pressing Start';
        amnesiaTileActionsDiv.replaceChildren(amnesiaWarningText, amnesiaStartButton);
    }
    else if (state === 'Connecting') {
        amnesiaTileCircle.className = CircleStyleBlue;
        amnesiaTileDesc.innerText = 'Connecting...';
        amnesiaWarningText.innerText = 'Ensure the game is running and in focus';
        amnesiaTileActionsDiv.replaceChildren(amnesiaWarningText);
    }
    else if (state === 'Connected') {
        amnesiaTileCircle.className = CircleStyleGreen;
        amnesiaTileDesc.innerText = 'Connected';

        amnesiaStopButton.disabled = false;
        amnesiaTileActionsDiv.replaceChildren(amnesiaStopButton);
    }
    else if (state === 'Failed') {
        amnesiaTileCircle.className = CircleStyleRed;
        amnesiaTileDesc.innerText = 'ERROR';
        amnesiaErrorText.innerText = message;
        amnesiaStartButton.disabled = false;
        amnesiaStartButton.innerText = "Retry";
        amnesiaTileActionsDiv.replaceChildren(amnesiaErrorText, amnesiaStartButton);
    }
}

function setTwitchTileState(state) {
    console.log("Twitch bot state: ", state);
    twitchState = state;
    onAnyStateChanged();

    if (state === 'Disconnected') {
        twitchTileCircle.className = CircleStyleDefault;
        twitchTileDesc.innerText = 'Not running';
        twitchStartButton.disabled = false;
        twitchWarningText.innerText = 'Don\'t forget to set your Bot Config in the settings';
        twitchTileActionsDiv.replaceChildren(twitchWarningText, twitchStartButton);
    }
    else if (state == 'Connecting')
    {
        twitchTileCircle.className = CircleStyleBlue;
        twitchTileDesc.innerText = 'Connecting...';
        twitchTileActionsDiv.replaceChildren();
    }
    else if (state == 'Connected') {
        twitchTileCircle.className = CircleStyleGreen;
        twitchTileDesc.innerText = 'Connected';

        twitchStopButton.disabled = false;
        twitchTileActionsDiv.replaceChildren(twitchStopButton);
    }
    else if (state === 'Failed') {
        setTwitchError('Please retry to see the error details')
    }
}

function setTwitchError(error) {
    twitchTileCircle.className = CircleStyleRed;
    twitchTileDesc.innerText = 'ERROR';
    twitchErrorText.innerText = error;
    twitchStartButton.disabled = false;
    twitchStartButton.innerText = 'Retry';
    twitchTileActionsDiv.replaceChildren(twitchErrorText, twitchStartButton);
}

function setChaosError(error) {
    chaosErrorLabel.style.visibility = 'visible';
    chaosErrorLabel.innerText = error;
    onAnyStateChanged();
}
