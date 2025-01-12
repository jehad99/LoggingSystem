import { Component, OnInit } from '@angular/core';
import { LogService } from '../../services/log.service';
import { Log, LogResponse } from '../../models/log.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-log-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './log-viewer.component.html',
  styleUrls: ['./log-viewer.component.css']
})
export class LogViewerComponent implements OnInit {
  logs: Log[] = [];
  startDate: string = '';
  endDate: string = '';
  currentPage = 1;
  pageSize = 10;
  totalRecords = 0;
  totalPages = 0;
  Math = Math;

  constructor(private logService: LogService) {
    // Set default date range to last 24 hours
    const end = new Date();
    const start = new Date(end);
    start.setDate(start.getDate() - 1);
    
    this.startDate = start.toISOString().slice(0, 16);
    this.endDate = end.toISOString().slice(0, 16);
  }

  ngOnInit() {
    this.loadLogs();
  }

  loadLogs() {
    if (!this.startDate || !this.endDate) return;
    
    this.logService.getLogs(
      new Date(this.startDate),
      new Date(this.endDate),
      this.currentPage,
      this.pageSize
    ).subscribe((response: LogResponse) => {
      this.logs = response.data;
      this.totalRecords = response.totalRecords;
      this.totalPages = Math.ceil(this.totalRecords / this.pageSize);
    });
  }

  getLevelClass(level: string): string {
    return `level-${level.toLowerCase()}`;
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadLogs();
    }
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadLogs();
    }
  }
}