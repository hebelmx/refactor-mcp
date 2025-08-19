import * as vscode from 'vscode';
import { execFile } from 'child_process';
import * as path from 'path';

function getWorkspaceFolder(): string | undefined {
    const folder = vscode.workspace.workspaceFolders?.[0];
    if (!folder) {
        vscode.window.showErrorMessage('ExxerFactorMCP requires an open workspace containing ExxerFactorMCP.ConsoleApp');
        return undefined;
    }
    return folder.uri.fsPath;
}


function runJson(toolName: string, json: string): Thenable<string> {
    const config = vscode.workspace.getConfiguration();
    const dotnetPath = config.get<string>('ExxerFactorMcp.dotnetPath', 'dotnet');
    const workspaceFolder = getWorkspaceFolder();
    if (!workspaceFolder) {
        return Promise.reject('No workspace');
    }
    const projectPath = path.join(workspaceFolder, 'ExxerFactorMCP.ConsoleApp');
    const commandArgs = ['run', '--project', projectPath, '--', '--json', toolName, json];
    return new Promise((resolve, reject) => {
        execFile(dotnetPath, commandArgs, { cwd: workspaceFolder }, (err, stdout, stderr) => {
            if (err) {
                reject(stderr || err.message);
            } else {
                resolve(stdout);
            }
        });
    });
}

async function getAvailableTools(): Promise<string[]> {
    try {
        const output = await runJson('ListTools', '{}');
        return output
            .split(/\r?\n/)
            .map(l => l.trim())
            .filter(l => l.length > 0);
    } catch {
        return [];
    }
}

function toPascalCase(name: string): string {
    return name
        .split('-')
        .map(part => part.charAt(0).toUpperCase() + part.slice(1))
        .join('');
}

export function activate(context: vscode.ExtensionContext) {
    const disposable = vscode.commands.registerCommand('ExxerFactorMcp.extractMethod', async () => {
        const editor = vscode.window.activeTextEditor;
        if (!editor) {
            return vscode.window.showWarningMessage('No active editor');
        }
        const document = editor.document.uri.fsPath;
        const selection = editor.selection;
        const start = documentPosition(selection.start);
        const end = documentPosition(selection.end);

        const methodName = await vscode.window.showInputBox({ prompt: 'Name for the new method' });
        if (!methodName) {
            return;
        }

        const range = `${start}-${end}`;
        const json = JSON.stringify({ solutionPath: '', filePath: document, selectionRange: range, methodName });

        try {
            await vscode.window.withProgress({ location: vscode.ProgressLocation.Notification, title: 'ExxerFactorMCP: Extract Method' }, async () => {
                const output = await runJson('ExtractMethod', json);
                vscode.window.showInformationMessage('ExxerFactorMCP completed');
                console.log(output);
            });
        } catch (err: any) {
            vscode.window.showErrorMessage(`ExxerFactorMCP failed: ${err}`);
        }
    });

    const runTool = vscode.commands.registerCommand('ExxerFactorMcp.runTool', async () => {
        const tools = await getAvailableTools();
        if (tools.length === 0) {
            vscode.window.showErrorMessage('Failed to retrieve tool list');
            return;
        }

        const toolPick = await vscode.window.showQuickPick(tools, { placeHolder: 'Select ExxerFactorMCP tool' });
        if (!toolPick) {
            return;
        }

        const paramJson = await vscode.window.showInputBox({ prompt: 'Tool parameters as JSON' });
        if (paramJson === undefined) {
            return;
        }

        const pascal = toPascalCase(toolPick);
        try {
            await vscode.window.withProgress({ location: vscode.ProgressLocation.Notification, title: `ExxerFactorMCP: ${toolPick}` }, async () => {
                const output = await runJson(pascal, paramJson);
                vscode.window.showInformationMessage('ExxerFactorMCP completed');
                console.log(output);
            });
        } catch (err: any) {
            vscode.window.showErrorMessage(`ExxerFactorMCP failed: ${err}`);
        }
    });

    context.subscriptions.push(runTool);

    context.subscriptions.push(disposable);
}

function documentPosition(pos: vscode.Position): string {
    return `${pos.line + 1}:${pos.character + 1}`;
}

export function deactivate() {}
