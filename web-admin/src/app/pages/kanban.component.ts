import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { Application, Job, STAGES, Stage } from '../core/models';
import { ToastService } from '../core/toast.service';

// The main recruiter screen: six columns, drag a card to move a candidate.
// On phones (no drag) every card also has a "Move to" dropdown.
@Component({
  selector: 'app-kanban',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <h1>Pipeline</h1>
    <div class="row" style="margin-bottom:16px">
      <div>
        <label>Job</label>
        <select [(ngModel)]="jobId" (change)="load()">
          <option [ngValue]="null">All jobs</option>
          @for (job of jobs; track job.id) { <option [ngValue]="job.id">{{ job.title }}</option> }
        </select>
      </div>
      <div><label>Search candidate</label><input [(ngModel)]="search" (keyup.enter)="load()" placeholder="Name or email"></div>
      <button class="btn ghost" (click)="load()">Search</button>
    </div>

    <div class="board">
      @for (stage of stages; track stage) {
        <div class="column" [class.over]="overStage === stage"
             (dragover)="allowDrop($event, stage)" (dragleave)="overStage = null" (drop)="drop(stage)">
          <h3><span>{{ stage }}</span><span class="chip">{{ inStage(stage).length }}</span></h3>
          @for (app of inStage(stage); track app.id) {
            <div class="kcard" [draggable]="canMove" (dragstart)="dragStart(app, $event)">
              <a class="name" [routerLink]="['/candidates', app.id]">{{ app.candidateName }}</a>
              <span class="muted" style="font-size:.8rem">{{ app.jobTitle }}</span>
              <div style="display:flex;gap:6px;margin-top:6px;align-items:center">
                <span class="chip" [class]="'chip ' + scoreClass(app.matchScore)">Match {{ app.matchScore }}%</span>
                <span class="muted" style="font-size:.75rem">{{ app.daysInStage }}d in stage</span>
              </div>
              @if (canMove && app.stage !== 'Hired') {
                <select (change)="moveTo(app, $any($event.target).value); $any($event.target).value = ''">
                  <option value="">Move to…</option>
                  @for (s of moveTargets(app); track s) { <option [value]="s">{{ s }}</option> }
                </select>
              }
            </div>
          } @empty {
            <div class="muted" style="font-size:.8rem;text-align:center;padding:12px 0">No candidates</div>
          }
        </div>
      }
    </div>
  `
})
export class KanbanComponent implements OnInit {
  private api = inject(ApiService);
  private toast = inject(ToastService);
  private auth = inject(AuthService);

  stages = STAGES;
  jobs: Job[] = [];
  apps: Application[] = [];
  jobId: number | null = null;
  search = '';
  overStage: Stage | null = null;
  private dragged: Application | null = null;

  get canMove(): boolean {
    return this.auth.hasRole('Admin', 'Recruiter');
  }

  ngOnInit(): void {
    this.api.jobs('', '', 1, 100).subscribe(r => (this.jobs = r.items));
    this.load();
  }

  load(): void {
    this.api.applications(this.jobId, this.search).subscribe(r => (this.apps = r.items));
  }

  inStage(stage: Stage): Application[] {
    return this.apps.filter(a => a.stage === stage);
  }

  moveTargets(app: Application): Stage[] {
    return this.stages.filter(s => s !== app.stage && s !== 'Hired');
  }

  scoreClass(score: number): string {
    return score >= 70 ? 'good' : score >= 40 ? 'mid' : 'bad';
  }

  dragStart(app: Application, event: DragEvent): void {
    this.dragged = app;
    event.dataTransfer?.setData('text/plain', String(app.id));
  }

  allowDrop(event: DragEvent, stage: Stage): void {
    event.preventDefault();
    this.overStage = stage;
  }

  drop(stage: Stage): void {
    this.overStage = null;
    if (this.dragged) {
      this.moveTo(this.dragged, stage);
    }
    this.dragged = null;
  }

  moveTo(app: Application, target: string): void {
    const stage = target as Stage;
    if (!stage || stage === app.stage || !this.canMove) {
      return;
    }

    let reason: string | undefined;
    if (stage === 'Rejected') {
      reason = prompt('Why is this candidate being rejected?')?.trim();
      if (!reason) {
        return;
      }
    }

    // The card only moves after the API accepts the move. If a rule blocks it, the card stays
    // where it was and the API message is shown by the error interceptor.
    this.api.moveStage(app.id, stage, reason).subscribe(updated => {
      this.apps = this.apps.map(a => (a.id === updated.id ? updated : a));
      this.toast.success(`${updated.candidateName} moved to ${updated.stage}.`);
    });
  }
}
