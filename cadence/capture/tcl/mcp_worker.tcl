# MCP Capture worker template
# Polls queue folders and dispatches only whitelisted actions.

proc mcp_load_job {jobPath} {
    # TODO: parse JSON file into a Tcl dict using the approved JSON package.
    return {}
}

proc mcp_write_result {resultPath resultDict} {
    # TODO: serialize deterministic result JSON.
}

proc mcp_dispatch_job {jobDict} {
    set action [dict get $jobDict action]

    switch -- $action {
        "create_symbol" {
            return [mcp_create_symbol_from_json $jobDict]
        }
        "verify_symbol" {
            return [mcp_verify_symbol $jobDict]
        }
        default {
            error "Unsupported Capture action: $action"
        }
    }
}

proc mcp_poll_capture_queue {queueRoot} {
    # TODO: iterate pending jobs, move them to running, execute,
    # then write result JSON into done/failed.
    # Never execute raw Tcl from queue payloads.
}
