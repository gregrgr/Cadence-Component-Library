; MCP Allegro worker template
; Polls queue folders and dispatches only whitelisted actions.

(defun mcpDispatchJob (job)
  (let ((action (cdr (assoc 'action job))))
    (cond
      ((equal action "create_footprint") (mcpCreateFootprintFromJson job))
      ((equal action "verify_footprint") (mcpVerifyFootprint job))
      (t (error "Unsupported Allegro action: %L" action)))))

(defun mcpPollAllegroQueue (queueRoot)
  ; TODO: iterate pending jobs, move them to running, execute,
  ; then write deterministic result JSON into done/failed.
  ; Never execute raw SKILL from queue payloads.
  t)
