const twitchChaosButton = document.getElementById("TwitchChaosBtn");
const localChaosButton = document.getElementById("LocalChaosBtn");

const widgetsWrapper = document.getElementById("WidgetsWrapper");

/* SignalR */
const connection = new signalR.HubConnectionBuilder().withUrl("/statushub").build();
connection.start().catch(function (err) {
    return console.error(err.toString());
});

connection.on("PayloadLoadingFailed", onPayloadLoadError);
connection.on("PayloadStateChanged", onPayloadStateChanged);

connection.on("AmnesiaFailed", onAmnesiaError);
connection.on("AmnesiaStateChanged", onAmnesiaStateChanged);

connection.on("TwitchFailed", onTwitchError);
connection.on("TwitchStateChanged", onTwitchStateChanged);

const WidgetStates = {
    Payloads: {
        status: "Ready",
        message: ""
    },
    Amnesia: {
        status: "Disabled",
        message: ""
    },
    Twitch: {
        status: "Ready",
        message: ""
    }
};

function SetWidgetState(widgetName, state) {
    switch (widgetName.toLowerCase()) {
        case "payloads":
            WidgetStates.Payloads.status = state;
            WidgetStates.Amnesia.status = state.toLowerCase() === "success"
                ? "ready"
                : "disabled";
            WidgetStates.Twitch.status = state.toLowerCase() === "success"
                ? "ready"
                : "disabled";
            break;
        case "amnesia":
            WidgetStates.Amnesia.status = state;
            if (state.toLowerCase() === "success") {
                // TODO: Enable Local Chaos
            } else {
                // TODO: Disable Local Chaos
                // TODO: Disable Twitch Chaos
            }
            break;
        case "twitch":
            WidgetStates.Twitch.status = state;
            if (state.toLowerCase() === "success") {
                // TODO: Enable Twitch Chaos
            } else {
                // TODO: Disable Twitch Chaos
            }
            break;
        default:
    }

    if (WidgetStates.Amnesia.status.toLowerCase() !== "success") {
        twitchChaosButton.disabled = true;
        localChaosButton.disabled = true;
    } else {
        localChaosButton.disabled = false;
        twitchChaosButton.disabled = WidgetStates.Twitch.status.toLowerCase() !== "success";
    }

    _renderWidgetStates();
}

function _renderWidgetStates() {
    widgetsWrapper.innerHTML = "";

    _renderWidget(`payload-state-${WidgetStates.Payloads.status.toLowerCase()}`, element => {
        switch (WidgetStates.Payloads.status.toLowerCase()) {
            case "ready":
                element
                    .getElementById("BtnLoad")
                    .addEventListener("click", onLoadPayloadsClicked);
                break;
            case "progress":
                // NO-OP
                break;
            case "error":
                element
                    .getElementById("BtnRetry")
                    .addEventListener("click", onLoadPayloadsClicked);
                element
                    .getElementById("ErrorSpan")
                    .innerText = WidgetStates.Payloads.message;
                break;
            case "success":
                element
                    .getElementById("BtnReload")
                    .addEventListener("click", onLoadPayloadsClicked);
                break;
            default:
        }
    });

    _renderWidget(`game-state-${WidgetStates.Amnesia.status.toLowerCase()}`, element => {
        switch (WidgetStates.Amnesia.status.toLowerCase()) {
            case "disabled":
                // NO-OP
                break;
            case "ready":
                element
                    .getElementById("BtnConnectGame")
                    .addEventListener("click", onConnectGameClicked);
                break;
            case "progress":
                // NO-OP
                break;
            case "error":
                element
                    .getElementById("BtnRetryGame")
                    .addEventListener("click", onConnectGameClicked);
                element
                    .getElementById("ErrorSpan")
                    .innerText = WidgetStates.Amnesia.message;
                break;
            case "success":
                element
                    .getElementById("BtnDisconnectGame")
                    .addEventListener("click", onDisconnectGameClicked);
                break;
            default:
        }
    });

    _renderWidget(`twitch-state-${WidgetStates.Twitch.status.toLowerCase()}`, element => {
        switch (WidgetStates.Twitch.status.toLowerCase()) {
            case "ready":
                element
                    .getElementById("BtnConnect")
                    .addEventListener("click", onTwitchConnectClicked);
                break;
            case "disabled":
                // NO-OP
                break;
            case "progress":
                // NO-OP
                break;
            case "error":
                element
                    .getElementById("BtnRetry")
                    .addEventListener("click", onTwitchConnectClicked);
                element
                    .getElementById("ErrorSpan")
                    .innerText = WidgetStates.Twitch.message;
                break;
            case "success":
                element
                    .getElementById("BtnDisconnect")
                    .addEventListener("click", onTwitchDisconnectClicked);
                break;
            default:
        }
    });
}

function onLoadPayloadsClicked(event) {
    console.log("onLoadPayloadsClicked");
    event.target.disabled = true;
    connection.invoke("LoadPayloads").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

function onConnectGameClicked(event) {
    console.log("onConnectGameClicked");
    event.target.disabled = true;
    connection.invoke("StartAmnesiaClient").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

function onDisconnectGameClicked(event) {
    console.log("onDisconnectGameClicked");
    event.target.disabled = true;
    connection.invoke("StopAmnesiaClient").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

function onTwitchConnectClicked(event) {
    console.log("onTwitchConnectClicked");
    event.target.disabled = true;
    connection.invoke("StartTwitchBot").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

function onTwitchDisconnectClicked(event) {
    console.log("onTwitchDisconnectClicked");
    event.target.disabled = true;
    connection.invoke("StopTwitchBot").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

function startTwitchPoll() {
    twitchChaosButton.disabled = true;
    localChaosButton.disabled = true;
    connection.invoke("StartTwitchPollChaos").catch(function (err) {
        return console.error(err.toString());
    });
}

function startLocalChaos() {
    twitchChaosButton.disabled = true;
    localChaosButton.disabled = true;
    connection.invoke("StartLocalChaos").catch(function (err) {
        return console.error(err.toString());
    });
}

/**
 * Renders a widget by cloning a template and applying a custom action to it.
 *
 * @param {string} templateId - The ID of the <template> element to clone.
 * @param {(clone: DocumentFragment) => void} actionFunc - A callback function that modifies the cloned content before it's appended.
 */
function _renderWidget(templateId, actionFunc) {
    const template = document.getElementById(templateId);

    if (!template) {
        console.error("Template with the following ID was not found:", templateId);
        return;
    }

    if (typeof actionFunc !== 'function') {
        console.error("Expected actionFunc to be a function.");
        return;
    }

    const clone = template.content.cloneNode(true);
    actionFunc(clone);
    widgetsWrapper.appendChild(clone);
}

/* SignalR Callbacks */
function onPayloadLoadError(error) {
    WidgetStates.Payloads.message = error;
    SetWidgetState("payloads", "error");
}

function onPayloadStateChanged(newState) {
    SetWidgetState("payloads", newState);
}

function onAmnesiaError(error) {
    WidgetStates.Amnesia.message = error;
    SetWidgetState("amnesia", "error");
}

function onAmnesiaStateChanged(newState) {
    SetWidgetState("amnesia", newState);
}

function onTwitchError(error) {
    WidgetStates.Twitch.message = error;
    SetWidgetState("twitch", "error");
}

function onTwitchStateChanged(newState) {
    SetWidgetState("twitch", newState);
}
