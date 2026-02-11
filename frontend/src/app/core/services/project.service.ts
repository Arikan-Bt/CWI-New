import { Injectable, signal } from '@angular/core';

export type ProjectType = 'CWI' | 'AWC';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private readonly STORAGE_KEY = 'selected_project';

  // Varsayılan olarak CWI seçili
  currentProject = signal<ProjectType>(
    (localStorage.getItem(this.STORAGE_KEY) as ProjectType) || 'CWI'
  );

  setProject(project: ProjectType) {
    this.currentProject.set(project);
    localStorage.setItem(this.STORAGE_KEY, project);
  }

  getProject(): ProjectType {
    return this.currentProject();
  }
}
