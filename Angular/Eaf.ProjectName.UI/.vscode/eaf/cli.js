'use strict';

// Libs
const colors = require('colors');
const shell = require('shelljs');
const args = require('command-line-args');
const usage = require('command-line-usage');

//Command options
const commandDefinitions = [
    { name: 'command', alias: 'c', type: String, defaultOption: true },
    { name: 'port', alias: 'p', type: Number }
];

const options = args(commandDefinitions);

const optionsParsed = {
    command: options.command == undefined ? 'run' : options.command,
    port: options.port == undefined ? '8000' : options.port,
};

const commandUsageText = [
    {
        content: [
            '███████╗ █████╗ ███████╗'.yellow,
            '██╔════╝██╔══██╗██╔════╝'.yellow,
            '█████╗  ███████║█████╗  '.yellow,
            '██╔══╝  ██╔══██║██╔══╝  '.yellow,
            '███████╗██║  ██║██║     '.yellow,
            '╚══════╝╚═╝  ╚═╝╚═╝     '.yellow,
            'Enterprise Application Foundation'.yellow,
            'Commandline Interface - v2'.yellow,
        ]
    },
    {
        header: 'Synopsis',
        content: 'npm ' + 'run'.cyan + ' eaf'.yellow + ' -- ' + '<command>'.cyan + ' <options>'.green
    },
    {
        header: 'Command List',
        content: [
            { name: 'build'.cyan, summary: 'Build project.' },
            { name: 'install'.cyan, summary: 'Install NPM Dependece with legacy-peer-deps.' },
            { name: 'help'.cyan, summary: 'Display the usage guide.' },
            { name: 'run'.cyan, summary: 'Run project in browser device using live reload mode.' },
            { name: 'service-update'.cyan, summary: 'Update service-proxies.ts file with latest Api contracts.' }
        ]
    },
    {
        header: 'Options',
        optionList: [
            {
                name: 'port'.green,
                description: 'Defines host port. Default is 8000'
            }
        ]
    },
    {
        header: 'Examples',
        content: [
            'npm ' + 'run'.cyan + ' eaf'.yellow,
            'Runs application in browser application on default port',
            '',
            'npm ' + 'run'.cyan + ' eaf'.yellow + ' build'.cyan,
            'Build application into default folder.'
        ]
    },
    {
        header: 'Dependencies',
        content: [
            'Node'.cyan + ' 16'.yellow + ' LTS'.cyan + ' (https://nodejs.org/en/blog/release/v16.16.0/)',
            'npm'.cyan + ' 8.5.5'.yellow,
            '@angular/cli'.cyan + ' ^12'.yellow,
        ]
    },
]

// Commands
function build() {
    const result = exec('ng build --prod=true --output-hashing=all');
    if (result.code != 0) {
        throw new Error('Error on Eaf build');
    }
}

function install() {
    info('npm install deps');
    exec('npm install -g @angular/cli@12 --force');
    exec('npm install -g colors');
    exec('npm install -g shelljs');
    exec('npm install -g command-line-args');
    exec('npm install -g command-line-usage');
    exec('npm install --legacy-peer-deps --force');
}

function help() {
    console.log(usage(commandUsageText));
}

function run() {
    info('Starting live reload server on port ' + optionsParsed.port + '.');
    exec('ng serve --host localhost --port ' + optionsParsed.port);
}

function serviceUpdate() {
    info('Generating service proxy file.');
    exec('node ./node_modules/nswag/bin/nswag.js run');
}

function error(message) {
    console.log('Eaf :   '.yellow + message.red);
}

function info(message) {
    console.log('Eaf :   '.yellow + message.green);
}

function log(message) {
    console.log('Eaf :   '.gray + message.gray);
}

function exec(command) {
    log('Running command: ' + command);
    return shell.exec(command);
}

// Command implementation
function execute() {
    log('options: ' + JSON.stringify(options));
    log('optionsParsed: ' + JSON.stringify(optionsParsed));

    switch (optionsParsed.command.toString().toLowerCase()) {
        case "build":
            build();
            break;
        case "help":
            help();
            break;
        case "run":
            run();
            break;
        case "service-update":
            serviceUpdate();
            break;
        case "install":
            install();
            break;
        default:
            error('Unknown command: ' + optionsParsed.command);
            break;
    }
}

// Command runner
execute();
