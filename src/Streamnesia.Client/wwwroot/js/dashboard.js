const CircleStyleDefault = "rounded-circle bg-secondary";
const CircleStyleBlue = "rounded-circle bg-primary";
const CircleStyleGreen = "rounded-circle bg-success";
const CircleStyleRed = "rounded-circle bg-danger";

const amnesiaTile = document.getElementById('AmnesiaTile');
const amnesiaTileCircle = document.getElementById('AmnesiaTileCircle');
const amnesiaTileDesc = document.getElementById('AmnesiaTileDesc');
const amnesiaTileActionsDiv = document.getElementById('AmnesiaTileActionsDiv');

const splashScreen = document.getElementById("SplashScreen");

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

function startChaosTest() {
    connection.invoke("StartLocalChaos").catch(function (err) {
        return console.error(err.toString());
    });
}

function startTwitchPoll() {
    connection.invoke("StartTwitchPollChaos").catch(function (err) {
        return console.error(err.toString());
    });
}

function startTwitchBot() {
    connection.invoke("StartTwitchBot").catch(function (err) {
        return console.error(err.toString());
    });
}

const amnesiaStartButton = document.createElement('button');
amnesiaStartButton.classList.add('btn', 'btn-outline-success', 'btn-sm', 'd-block');
amnesiaStartButton.innerText = 'Connect';
amnesiaStartButton.addEventListener('click', onAmnesiaConnectPressed);

const amnesiaStopButton = document.createElement('button');
amnesiaStopButton.classList.add('btn', 'btn-outline-danger', 'btn-sm', 'd-block');
amnesiaStopButton.innerText = 'Disconnect';
amnesiaStopButton.addEventListener('click', onAmnesiaDisconnectPressed);

const amnesiaWarningText = document.createElement('small');
amnesiaWarningText.classList.add('text-warning');

const amnesiaErrorText = document.createElement('small');
amnesiaErrorText.classList.add('text-danger');

const connection = new signalR.HubConnectionBuilder().withUrl("/statushub").build();
connection.start().catch(function (err) {
    return console.error(err.toString());
});

// TODO: Block until SignalR connects
connection.on("AmnesiaClientStateChanged", setAmnesiaTileState);

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
