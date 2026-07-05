(function () {
  const h = React.createElement;

  const initialState = {
    loading: true,
    currentUser: null,
    activeView: "login",
    message: null,
    data: {
      employees: [],
      projects: [],
      leaveTypes: [],
      roles: [],
      hrPolicy: {
        allowHalfDayLeave: false
      }
    },
    authForm: {
      username: "",
      password: ""
    },
    employeeForm: {
      employeeCode: "",
      fullName: "",
      email: "",
      department: "",
      designation: "",
      jobRole: "",
      employmentType: "Full-time",
      location: "",
      salaryStructureDetails: "",
      joinDate: currentDateInputValue(),
      role: 1,
      primaryTeamId: "",
      teamIds: []
    },
    leaveForm: {
      employeeId: "",
      leaveTypeId: "",
      startDate: "",
      endDate: "",
      isHalfDay: false,
      reason: ""
    },
    hrPolicyForm: {
      allowHalfDayLeave: false
    },
    roleForm: {
      label: "",
      baseRole: 1
    },
    onboardingForm: {
      employeeId: "",
      panNumber: "",
      aadhaarNumber: "",
      hasPriorExperience: true,
      previousEmployerName: "",
      yearsOfExperience: "",
      relievingEmailForwarded: false,
      documents: []
    },
    projectForm: {
      name: ""
    },
    teamForm: {
      name: "",
      projectId: "",
      leadId: ""
    },
    excelUpload: {
      fileName: "",
      result: null
    },
    existingEmployeeUpload: {
      fileName: "",
      result: null
    },
    reviewer: {
      reviewerId: "",
      reviewerName: "",
      requests: [],
      recentDecisions: [],
      loading: false
    },
    myLeaves: {
      employeeId: "",
      requests: [],
      loading: false
    }
  };

  const welcomeSlides = [
    { image: "/assets/warli-carousel-1.png", title: "Teams in flow" },
    { image: "/assets/warli-carousel-2.png", title: "Onboarding and growth" },
    { image: "/assets/warli-carousel-3.png", title: "Work and wellbeing" }
  ];

  function reducer(state, action) {
    if (!state) return initialState;

    switch (action.type) {
      case "workspace/loading":
        return { ...state, loading: true };
      case "workspace/loadFailed":
        return { ...state, loading: false };
      case "workspace/loaded":
        const availableReviewers = reviewerOptions(action.payload.employees);
        const currentEmployeeId = state.currentUser ? String(state.currentUser.employeeId) : "";
        const currentReviewerId = state.currentUser && canApproveRole(state.currentUser.role)
          ? String(state.currentUser.employeeId)
          : "";
        const currentReviewerStillValid = availableReviewers.some(function (employee) {
          return String(employee.id) === String(state.reviewer.reviewerId);
        });
        return {
          ...state,
          loading: false,
          data: action.payload,
          employeeForm: {
            ...state.employeeForm,
            joinDate: state.employeeForm.joinDate || currentDateInputValue(),
            primaryTeamId: state.employeeForm.primaryTeamId || firstTeamId(action.payload),
            teamIds: state.employeeForm.teamIds.length ? state.employeeForm.teamIds : [firstTeamId(action.payload)].filter(Boolean)
          },
          leaveForm: {
            ...state.leaveForm,
            employeeId: currentEmployeeId || state.leaveForm.employeeId || firstId(action.payload.employees),
            leaveTypeId: state.leaveForm.leaveTypeId || firstId(action.payload.leaveTypes)
          },
          hrPolicyForm: {
            allowHalfDayLeave: Boolean(action.payload.hrPolicy && action.payload.hrPolicy.allowHalfDayLeave)
          },
          onboardingForm: {
            ...state.onboardingForm,
            employeeId: currentEmployeeId || state.onboardingForm.employeeId || firstId(action.payload.employees)
          },
          teamForm: {
            ...state.teamForm,
            projectId: state.teamForm.projectId || firstId(action.payload.projects),
            leadId: state.teamForm.leadId || firstId(action.payload.employees)
          },
          reviewer: {
            ...state.reviewer,
            reviewerId: currentReviewerId || (currentReviewerStillValid
              ? state.reviewer.reviewerId
              : firstId(availableReviewers))
          },
          myLeaves: {
            ...state.myLeaves,
            employeeId: currentEmployeeId || state.myLeaves.employeeId
          }
        };
      case "auth/update":
        return {
          ...state,
          authForm: { ...state.authForm, [action.field]: action.value }
        };
      case "auth/loginSuccess":
        return {
          ...state,
          currentUser: action.payload,
          activeView: defaultViewForUser(action.payload),
          authForm: {
            username: action.payload.username,
            password: ""
          },
          leaveForm: {
            ...state.leaveForm,
            employeeId: String(action.payload.employeeId)
          },
          onboardingForm: {
            ...state.onboardingForm,
            employeeId: String(action.payload.employeeId)
          },
          reviewer: {
            ...state.reviewer,
            reviewerId: canApproveRole(action.payload.role) ? String(action.payload.employeeId) : ""
          },
          myLeaves: {
            employeeId: String(action.payload.employeeId),
            requests: [],
            loading: false
          },
          message: null
        };
      case "auth/logout":
        return {
          ...state,
          currentUser: null,
          activeView: "login",
          authForm: {
            username: "",
            password: ""
          },
          reviewer: {
            reviewerId: "",
            reviewerName: "",
            requests: [],
            recentDecisions: [],
            loading: false
          },
          myLeaves: {
            employeeId: "",
            requests: [],
            loading: false
          },
          message: null
        };
      case "view/set":
        return { ...state, activeView: action.payload, message: null };
      case "message/set":
        return { ...state, message: action.payload };
      case "employeeForm/update":
      case "leaveForm/update":
      case "onboardingForm/update":
      case "projectForm/update":
      case "teamForm/update":
      case "hrPolicyForm/update":
      case "roleForm/update":
        return {
          ...state,
          [action.form]: { ...state[action.form], [action.field]: action.value }
        };
      case "employeeForm/toggleTeam": {
        const exists = state.employeeForm.teamIds.includes(action.teamId);
        const teamIds = exists
          ? state.employeeForm.teamIds.filter((id) => id !== action.teamId)
          : state.employeeForm.teamIds.concat(action.teamId);
        return {
          ...state,
          employeeForm: { ...state.employeeForm, teamIds: teamIds }
        };
      }
      case "employeeForm/reset":
        return {
          ...state,
          employeeForm: {
            employeeCode: "",
            fullName: "",
            email: "",
            department: "",
            designation: "",
            jobRole: "",
            employmentType: "Full-time",
            location: "",
            salaryStructureDetails: "",
            joinDate: currentDateInputValue(),
            role: 1,
            primaryTeamId: firstTeamId(state.data),
            teamIds: [firstTeamId(state.data)].filter(Boolean)
          }
        };
      case "leaveForm/reset":
        return {
          ...state,
          leaveForm: {
            employeeId: state.leaveForm.employeeId,
            leaveTypeId: state.leaveForm.leaveTypeId,
            startDate: "",
            endDate: "",
            isHalfDay: false,
            reason: ""
          }
        };
      case "roleForm/reset":
        return {
          ...state,
          roleForm: {
            label: "",
            baseRole: 1
          }
        };
      case "onboarding/load":
        return {
          ...state,
          onboardingForm: {
            employeeId: state.onboardingForm.employeeId,
            panNumber: action.payload.panNumber || "",
            aadhaarNumber: action.payload.aadhaarNumber || "",
            hasPriorExperience: action.payload.hasPriorExperience !== false,
            previousEmployerName: action.payload.previousEmployerName || "",
            yearsOfExperience: action.payload.yearsOfExperience === null || action.payload.yearsOfExperience === undefined ? "" : String(action.payload.yearsOfExperience),
            relievingEmailForwarded: Boolean(action.payload.relievingEmailForwarded),
            documents: action.payload.documents || []
          }
        };
      case "projectForm/reset":
        return { ...state, projectForm: { name: "" } };
      case "teamForm/reset":
        return {
          ...state,
          teamForm: {
            name: "",
            projectId: firstId(state.data.projects),
            leadId: firstId(state.data.employees)
          }
        };
      case "excelUpload/setFile":
        return {
          ...state,
          excelUpload: { ...state.excelUpload, fileName: action.payload, result: null }
        };
      case "excelUpload/result":
        return {
          ...state,
          excelUpload: { ...state.excelUpload, result: action.payload }
        };
      case "existingEmployeeUpload/setFile":
        return {
          ...state,
          existingEmployeeUpload: { ...state.existingEmployeeUpload, fileName: action.payload, result: null }
        };
      case "existingEmployeeUpload/result":
        return {
          ...state,
          existingEmployeeUpload: { ...state.existingEmployeeUpload, result: action.payload }
        };
      case "reviewer/set":
        return {
          ...state,
          reviewer: { ...state.reviewer, reviewerId: action.payload }
        };
      case "reviewer/loading":
        return {
          ...state,
          reviewer: { ...state.reviewer, loading: true }
        };
      case "reviewer/loaded":
        return {
          ...state,
          reviewer: {
            reviewerId: state.reviewer.reviewerId,
            reviewerName: action.payload.reviewerName,
            requests: action.payload.requests || [],
            recentDecisions: action.payload.recentDecisions || [],
            loading: false
          }
        };
      case "myLeaves/loading":
        return {
          ...state,
          myLeaves: { ...state.myLeaves, employeeId: action.employeeId || state.myLeaves.employeeId, loading: true }
        };
      case "myLeaves/loaded":
        return {
          ...state,
          myLeaves: {
            employeeId: action.employeeId || state.myLeaves.employeeId,
            requests: action.payload || [],
            loading: false
          }
        };
      default:
        return state;
    }
  }

  const store = Redux.createStore(reducer);

  function firstId(items) {
    return items && items.length ? String(items[0].id) : "";
  }

  function firstTeamId(data) {
    const project = data.projects.find(function (item) { return item.teams && item.teams.length; });
    return project ? String(project.teams[0].id) : "";
  }

  function allTeams(projects) {
    return projects.flatMap(function (project) {
      return project.teams.map(function (team) {
        return { ...team, projectName: project.name };
      });
    });
  }

  function reviewerOptions(employees) {
    return employees.filter(function (employee) {
      return canApproveRole(employee.role);
    });
  }

  function currentDateInputValue() {
    return new Date().toISOString().slice(0, 10);
  }

  function dateInputValue(value) {
    if (!value) return "";
    return new Date(value).toISOString().slice(0, 10);
  }

  function isHrRole(role) {
    return role === "HR" || role === "HRL2";
  }

  function isManagerRole(role) {
    return role === "Manager" || role === "ManagerL2";
  }

  function canApproveRole(role) {
    return role === "TeamLead" || isHrRole(role) || role === "OrganizationHead";
  }

  function roleLabel(role, fallback) {
    const labels = {
      Employee: "Software Engineer",
      TeamLead: "Team Lead",
      HR: "HR L1",
      HRL2: "HR L2",
      SeniorSoftwareEngineer: "Senior Software Engineer",
      Manager: "Technical Manager L1",
      ManagerL2: "Technical Manager L2",
      OrganizationHead: "Organization Head"
    };

    return cleanRoleLabel(fallback || labels[role] || role);
  }

  function cleanRoleLabel(label) {
    const replacements = {
      Employee: "Software Engineer",
      "Manager L1": "Technical Manager L1",
      "Manager L2": "Technical Manager L2"
    };

    return replacements[label] || label;
  }

  function roleImportance(employee) {
    if (!employee) return 0;
    const rank = {
      OrganizationHead: 800,
      HRL2: 700,
      HR: 650,
      ManagerL2: 600,
      Manager: 550,
      TeamLead: 500,
      SeniorSoftwareEngineer: 400,
      Employee: 300
    };

    return rank[employee.role] || 100;
  }

  function defaultViewForUser(user) {
    if (!user || !user.views || !user.views.length) {
      return "apply";
    }

    return user.views.includes("directory") ? "directory" : user.views[0];
  }

  function homeViewForUser(user) {
    if (!user || !user.views || !user.views.length) {
      return "login";
    }

    if (user.views.includes("hrHome")) {
      return "hrHome";
    }

    if (user.views.includes("overview")) {
      return "overview";
    }

    if (user.views.includes("apply")) {
      return "apply";
    }

    return defaultViewForUser(user);
  }

  function viewsForUser(user) {
    const labels = {
      hrHome: "HR home",
      hrControl: "HR control",
      apply: "My leave",
      onboarding: "My onboarding",
      register: "New employee onboarding",
      projects: "Projects & teams",
      balances: "Bulk uploads",
      directory: "Directory",
      review: "Leave review",
      overview: "Overview"
    };

    if (!user || !user.views) {
      return [];
    }

    return user.views.map(function (view) {
      return { id: view, label: labels[view] || view };
    });
  }

  function metaForView(view, user) {
    const roleText = user ? roleLabel(user.role, user.roleLabel) : "";
    const meta = {
      hrHome: {
        label: "HR",
        title: "Run HR operations in the right sequence",
        subtitle: "Start with company-controlled onboarding and reviews, then move to your own leave and employee self-service tasks."
      },
      hrControl: {
        label: "HR",
        title: "Control leave policy",
        subtitle: "Manage employee-facing leave options from one place."
      },
      apply: {
        label: roleText,
        title: "Plan time away with clarity",
        subtitle: "Apply for leave, see who will approve it, and keep the request tidy from the start."
      },
      onboarding: {
        label: roleText,
        title: "Complete onboarding part 2",
        subtitle: "This is the employee side of onboarding for identity details and supporting documents."
      },
      register: {
        label: "HR",
        title: "Complete company onboarding part 1",
        subtitle: "Capture official employee records, assign projects and teams, and trigger the employee onboarding invite."
      },
      projects: {
        label: "HR",
        title: "Keep the project structure current",
        subtitle: "Projects and teams define the approval path behind each employee."
      },
      balances: {
        label: "HR",
        title: "Maintain leave ledgers",
        subtitle: "Upload and adjust leave balances for the live workforce."
      },
      review: {
        label: "Reviewer",
        title: "Review employee leave requests",
        subtitle: "See the leave requests routed to you and respond from one clean queue."
      },
      directory: {
        label: roleText,
        title: "Read the organization at a glance",
        subtitle: "See primary teams, approvers, and growth across the directory."
      },
      overview: {
        label: "Leadership",
        title: "Read the organization at a glance",
        subtitle: "Track headcount, team ownership, and approval coverage."
      }
    };

    return meta[view] || meta.apply;
  }

  async function api(path, options) {
    const response = await fetch(path, {
      headers: { "Content-Type": "application/json" },
      ...options
    });
    const payload = await response.json().catch(function () { return {}; });
    if (!response.ok) {
      throw new Error(payload.message || payload.Message || "The request could not be completed.");
    }
    return payload;
  }

  async function loadWorkspace() {
    store.dispatch({ type: "workspace/loading" });
    try {
      const payload = await api("/api/workspace");
      store.dispatch({ type: "workspace/loaded", payload: payload });
    } catch (error) {
      store.dispatch({ type: "workspace/loadFailed" });
      store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
    }
  }

  async function login(authForm) {
    const payload = await api("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({
        username: authForm.username,
        password: authForm.password
      })
    });

    store.dispatch({ type: "auth/loginSuccess", payload: payload });
    await loadWorkspace();
  }

  async function loadReviewerRequests(reviewerId) {
    if (!reviewerId) {
      store.dispatch({ type: "reviewer/loaded", payload: { reviewerName: "", requests: [], recentDecisions: [] } });
      return;
    }
    store.dispatch({ type: "reviewer/loading" });
    try {
      const payload = await api("/api/leave/reviewer/" + reviewerId + "/requests");
      store.dispatch({
        type: "reviewer/loaded",
        payload: {
          reviewerName: payload.reviewer ? payload.reviewer.fullName : "",
          requests: payload.requests || [],
          recentDecisions: payload.recentDecisions || []
        }
      });
    } catch (error) {
      store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      store.dispatch({ type: "reviewer/loaded", payload: { reviewerName: "", requests: [], recentDecisions: [] } });
    }
  }

  async function loadMyLeaveRequests(employeeId) {
    if (!employeeId) {
      store.dispatch({ type: "myLeaves/loaded", employeeId: "", payload: [] });
      return;
    }

    store.dispatch({ type: "myLeaves/loading", employeeId: String(employeeId) });
    try {
      const payload = await api("/api/leave/employee/" + employeeId + "/requests");
      store.dispatch({
        type: "myLeaves/loaded",
        employeeId: String(employeeId),
        payload: payload.requests || []
      });
    } catch (error) {
      store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      store.dispatch({ type: "myLeaves/loaded", employeeId: String(employeeId), payload: [] });
    }
  }

  async function cancelLeaveRequest(requestId, employeeId) {
    const payload = await api("/api/leave/" + requestId + "/cancel", {
      method: "POST",
      body: JSON.stringify({
        employeeId: Number(employeeId)
      })
    });

    return payload;
  }

  async function loadOnboardingProfile(employeeId) {
    if (!employeeId) {
      store.dispatch({
        type: "onboarding/load",
        payload: {
          panNumber: "",
          aadhaarNumber: "",
          hasPriorExperience: true,
          previousEmployerName: "",
          yearsOfExperience: "",
          relievingEmailForwarded: false,
          documents: []
        }
      });
      return;
    }

    try {
      const payload = await api("/api/onboarding/" + employeeId);
      const profile = payload.profile || {};
      store.dispatch({
        type: "onboarding/load",
        payload: {
          panNumber: profile.panNumber,
          aadhaarNumber: profile.aadhaarNumber,
          hasPriorExperience: profile.hasPriorExperience,
          previousEmployerName: profile.previousEmployerName,
          yearsOfExperience: profile.yearsOfExperience,
          relievingEmailForwarded: profile.relievingEmailForwarded,
          documents: payload.documents || []
        }
      });
    } catch (error) {
      store.dispatch({
        type: "message/set",
        payload: { type: "error", text: error.message }
      });
    }
  }

  function selectedEmployee(state) {
    return state.data.employees.find(function (item) {
      return String(item.id) === String(state.leaveForm.employeeId);
    });
  }

  function currentEmployee(state) {
    if (!state.currentUser) return null;
    return state.data.employees.find(function (item) {
      return String(item.id) === String(state.currentUser.employeeId);
    }) || null;
  }

  function selectedRole(state) {
    return selectedRoleForValue(state.data.roles, state.employeeForm.role);
  }

  function selectedRoleForValue(roles, value) {
    return roles.find(function (role) {
      return String(role.id) === String(value);
    });
  }

  function baseRoleNameForRole(roles, role) {
    if (!role) return "";
    const baseRole = roles.find(function (item) {
      return !item.isCustom && Number(item.baseRoleId) === Number(role.baseRoleId);
    });

    return baseRole ? baseRole.name : role.name;
  }

  function approverForEmployee(employee, data) {
    if (!employee) return "Select an employee";
    if (employee.role === "OrganizationHead") {
      const hrForHead = data.employees.find(function (item) { return isHrRole(item.role); });
      return hrForHead ? hrForHead.fullName : "No active HR found";
    }
    if (isHrRole(employee.role)) {
      const orgHead = data.employees.find(function (item) { return item.role === "OrganizationHead"; });
      return orgHead ? orgHead.fullName : "No active organization head found";
    }
    if (isManagerRole(employee.role)) {
      const hr = data.employees.find(function (item) { return isHrRole(item.role); });
      return hr ? hr.fullName : "No active HR found";
    }
    return employee.primaryTeam && employee.primaryTeam.leadName
      ? employee.primaryTeam.leadName
      : "No primary team lead assigned";
  }

  function approverForRegistration(state) {
    const role = selectedRole(state);
    if (role && role.name === "OrganizationHead") {
      const hrForHead = state.data.employees.find(function (item) { return isHrRole(item.role); });
      return hrForHead ? hrForHead.fullName : "No HR employee found";
    }
    if (role && isHrRole(role.name)) {
      const orgHead = state.data.employees.find(function (item) { return item.role === "OrganizationHead"; });
      return orgHead ? orgHead.fullName : "No organization head found";
    }
    if (role && isManagerRole(role.name)) {
      const hr = state.data.employees.find(function (item) { return isHrRole(item.role); });
      return hr ? hr.fullName : "No HR employee found";
    }
    const teams = allTeams(state.data.projects);
    const team = teams.find(function (item) {
      return String(item.id) === String(state.employeeForm.primaryTeamId);
    });
    return team ? team.leadName : "Choose a primary team";
  }

  function balanceSummary(balance) {
    const allocated = Number(balance.allocatedLeaves || 0);
    const used = Number(balance.usedLeaves || 0);
    const remaining = Number(balance.remainingLeaves || 0);
    const usedPercent = allocated > 0 ? Math.min(100, Math.max(0, (used / allocated) * 100)) : 0;
    return {
      allocated: allocated,
      used: used,
      remaining: remaining,
      ringStyle: {
        background: "conic-gradient(var(--moss) 0deg " + (usedPercent * 3.6) + "deg, rgba(49, 95, 79, 0.12) " + (usedPercent * 3.6) + "deg 360deg)"
      }
    };
  }

  function formatDate(value) {
    if (!value) return "Not available";
    return new Date(value).toLocaleDateString();
  }

  function formatDays(value) {
    const days = Number(value || 0);
    return days + " day" + (days === 1 ? "" : "s");
  }

  function statusClass(status) {
    const normalized = String(status || "").toLowerCase();
    if (normalized === "approved") return "status-approved";
    if (normalized === "rejected") return "status-rejected";
    if (normalized === "cancelled") return "status-cancelled";
    return "status-pending";
  }

  function teamAssignmentSummary(employee) {
    if (!employee || !employee.teams || !employee.teams.length) {
      return "No project teams assigned";
    }

    return employee.teams
      .map(function (team) {
        return team.projectName + " / " + team.name;
      })
      .join(", ");
  }

  function hierarchyRows(state) {
    return state.data.employees.map(function (employee) {
      return {
        id: employee.id,
        roleRank: roleImportance(employee),
        employee: employee.fullName,
        employeeCode: employee.employeeCode,
        email: employee.email,
        role: roleLabel(employee.role, employee.roleLabel),
        department: employee.department,
        designation: employee.designation,
        jobRole: employee.jobRole,
        employmentType: employee.employmentType,
        location: employee.location,
        primaryTeam: employee.primaryTeam ? employee.primaryTeam.name : "Not assigned",
        projectsAndTeams: teamAssignmentSummary(employee),
        approver: approverForEmployee(employee, state.data),
        joiningDate: formatDate(employee.joinDate)
      };
    }).sort(function (left, right) {
      if (right.roleRank !== left.roleRank) {
        return right.roleRank - left.roleRank;
      }

      return left.employee.localeCompare(right.employee);
    });
  }

  function Field(props) {
    return h("div", { className: "field" },
      h("label", null, props.label),
      props.children
    );
  }

  function Message(props) {
    if (!props.message) return null;
    return h("div", { className: "message " + props.message.type }, props.message.text);
  }

  function LoadingScreen() {
    return h("div", { className: "loading-screen" },
      h("div", { className: "loading-mark" },
        h("div", { className: "loading-ring loading-ring-primary" }),
        h("div", { className: "loading-ring loading-ring-secondary" }),
        h("div", { className: "loading-core" },
          h("div", { className: "loading-symbol-frame" },
            h("img", {
              className: "loading-symbol",
              src: "/assets/relisoft-logo.jpg",
              alt: "ReliSoft symbol"
            })
          )
        )
      ),
      h("div", { className: "loading-copy" },
        h("strong", null, "ReliSoft Technologies"),
        h("span", null, "Preparing People Hub")
      )
    );
  }

  function App() {
    const [state, setState] = React.useState(store.getState());
    const [introReady, setIntroReady] = React.useState(false);

    React.useEffect(function () {
      const unsubscribe = store.subscribe(function () { setState(store.getState()); });
      loadWorkspace();
      return unsubscribe;
    }, []);

    React.useEffect(function () {
      const timer = window.setTimeout(function () {
        setIntroReady(true);
      }, 2600);

      return function () {
        window.clearTimeout(timer);
      };
    }, []);

    if (state.loading || !introReady) {
      return h(LoadingScreen);
    }

    function goHome() {
      store.dispatch({
        type: "view/set",
        payload: homeViewForUser(state.currentUser)
      });
    }

    return h("main", { className: "app-shell" },
      h("header", { className: "topbar" },
        h("div", {
          className: "brand",
          role: state.currentUser ? "button" : undefined,
          tabIndex: state.currentUser ? 0 : undefined,
          onClick: state.currentUser ? goHome : undefined,
          onKeyDown: state.currentUser ? function (event) {
            if (event.key === "Enter" || event.key === " ") {
              event.preventDefault();
              goHome();
            }
          } : undefined
        },
          h("img", {
            className: "brand-logo",
            src: "/assets/relisoft-logo.jpg",
            alt: "ReliSoft Technologies"
          }),
          h("div", null,
            h("div", { className: "brand-title" }, "ReliSoft Technologies"),
            h("div", { className: "meta" }, "People Hub")
          )
        )
      ),
      h(Message, { message: state.message }),
      !state.currentUser ? h(LoginPage, { state: state }) : h(RoleWorkspace, { state: state })
    );
  }

  function LoginPage(props) {
    const state = props.state;
    const [activeSlide, setActiveSlide] = React.useState(0);

    React.useEffect(function () {
      const timer = window.setInterval(function () {
        setActiveSlide(function (current) {
          return (current + 1) % welcomeSlides.length;
        });
      }, 4400);

      return function () {
        window.clearInterval(timer);
      };
    }, []);

    async function submitLogin(event) {
      event.preventDefault();
      try {
        await login(state.authForm);
      } catch (error) {
        store.dispatch({
          type: "message/set",
          payload: { type: "error", text: error.message }
        });
      }
    }

    return h("section", { className: "landing-shell" },
      h("div", { className: "landing-backdrop" },
        welcomeSlides.map(function (slide, index) {
          return h("div", {
            key: slide.image,
            className: "landing-slide" + (index === activeSlide ? " active" : "")
          },
            h("img", {
              className: "landing-slide-image",
              src: slide.image,
              alt: slide.title
            })
          );
        })
      ),
      h("div", { className: "landing-overlay" }),
      h("div", { className: "landing-grid" },
        h("div", { className: "landing-story" },
          h("div", { className: "landing-copy" },
            h("h1", null, "People operations, designed with clarity."),
            h("p", null, "Onboarding, leave, approvals, and workforce records brought together in one calm, reliable workspace for ReliSoft teams."),
            h("div", { className: "landing-pill-row" },
              ["Onboarding", "Leave", "Approvals", "Directory"].map(function (label) {
                return h("span", { key: label, className: "landing-pill" }, label);
              })
            )
          )
        ),
        h("div", { className: "landing-side" },
          h("section", { className: "panel login-panel" },
            h("div", { className: "panel-body login-panel-body" },
              h("div", { className: "login-panel-content" },
                h("span", { className: "login-panel-kicker" }, "Workspace access"),
                h("h2", { className: "login-panel-title" }, "Sign in"),
                h("p", { className: "login-panel-note" }, "Use your company username and password to enter the ReliSoft workspace."),
                h("form", { className: "login-form", onSubmit: submitLogin },
                  h("div", { className: "login-form-stack" },
                    h(Field, { label: "Username" },
                      h("input", {
                        value: state.authForm.username,
                        onChange: function (event) { store.dispatch({ type: "auth/update", field: "username", value: event.target.value }); },
                        placeholder: "Enter username",
                        required: true
                      })
                    ),
                    h(Field, { label: "Password" },
                      h("input", {
                        type: "password",
                        value: state.authForm.password,
                        onChange: function (event) { store.dispatch({ type: "auth/update", field: "password", value: event.target.value }); },
                        placeholder: "Enter password",
                        required: true
                      })
                    )
                  ),
                  h("div", { className: "login-submit-row" },
                    h("button", { className: "primary-button login-submit", type: "submit" }, "Sign in")
                  )
                ),
                h("div", { className: "login-panel-footer" },
                  h("span", { className: "login-panel-footer-label" }, "Secure sign-in for employees, HR, and leadership")
                )
              )
            )
          )
        )
      )
    );
  }

  function RoleWorkspace(props) {
    const state = props.state;
    const meta = metaForView(state.activeView, state.currentUser);
    const views = viewsForUser(state.currentUser);

    return h("section", { className: "role-layout" },
      h("aside", { className: "sidebar panel" },
        h("div", { className: "sidebar-section" },
          h("div", { className: "signed-in-chip sidebar-user" },
            h("strong", null, state.currentUser.fullName),
            h("span", null, roleLabel(state.currentUser.role, state.currentUser.roleLabel))
          )
        ),
        h("div", { className: "sidebar-section" },
          h("div", { className: "sidebar-label" }, "Workspace"),
          h("nav", { className: "side-nav" },
            views.map(function (view) {
              return h("button", {
                key: view.id,
                className: state.activeView === view.id ? "active" : "",
                onClick: function () { store.dispatch({ type: "view/set", payload: view.id }); }
              }, view.label);
            })
          )
        ),
        h("div", { className: "sidebar-section sidebar-footer" },
          h("button", {
            className: "quiet-button sidebar-logout",
            onClick: function () {
              store.dispatch({ type: "auth/logout" });
            }
          }, "Logout")
        )
      ),
      h("section", { className: "role-content" },
        h("div", { className: "role-banner panel" },
          h("div", { className: "panel-header" },
            h("div", null,
              h("div", { className: "role-kicker" }, meta.label),
              h("h2", { className: "panel-title" }, meta.title),
              h("div", { className: "panel-subtitle" }, meta.subtitle)
            )
          )
        ),
        h("div", { className: "workspace" },
          state.activeView === "hrHome" ? h(HrHome, { state: state }) : null,
          state.activeView === "hrControl" ? h(HrControlPanel, { state: state }) : null,
          state.activeView === "apply" ? h(LeaveHome, { state: state }) : null,
          state.activeView === "onboarding" ? h(EmployeeOnboarding, { state: state }) : null,
          state.activeView === "register" ? h(HrRegistration, { state: state }) : null,
          state.activeView === "projects" ? h(ProjectBuilder, { state: state }) : null,
          state.activeView === "balances" ? h(HrBulkUploads, { state: state }) : null,
          state.activeView === "review" ? h(ReviewerInbox, { state: state }) : null,
          state.activeView === "directory" ? h(Directory, { state: state }) : null,
          state.activeView === "overview" ? h(LeadershipOverview, { state: state }) : null
        )
      )
    );
  }

  function signal(value, label) {
    return h("div", { className: "signal" },
      h("strong", null, value),
      h("span", null, label)
    );
  }

  function HrHome(props) {
    const state = props.state;
    React.useEffect(function () {
      if (state.reviewer.reviewerId) {
        loadReviewerRequests(state.reviewer.reviewerId);
      }
    }, [state.reviewer.reviewerId]);

    const pendingRequests = state.reviewer.requests.length;
    const employees = state.data.employees.length;
    const teams = allTeams(state.data.projects).length;

    return h("section", { className: "workspace-stack" },
      h("section", { className: "panel" },
        h("div", { className: "panel-header" },
          h("div", null,
            h("h2", { className: "panel-title" }, "For the rest of the employees"),
            h("div", { className: "panel-subtitle" }, "Begin with company-controlled onboarding, leave review, and workforce records.")
          )
        ),
        h("div", { className: "panel-body" },
          h("div", { className: "action-grid" },
            h(ActionCard, {
              title: "New employee onboarding",
              description: "Complete HR-side part 1 with official fields, project assignment, and team allocation.",
              detail: employees + " employee records live",
              onClick: function () { store.dispatch({ type: "view/set", payload: "register" }); }
            }),
            h(ActionCard, {
              title: "Employee leaves review",
              description: "Review requests routed to HR and keep the approval queue moving.",
              detail: pendingRequests + " pending right now",
              onClick: function () { store.dispatch({ type: "view/set", payload: "review" }); }
            }),
            h(ActionCard, {
              title: "Projects and teams",
              description: "Maintain the structure that decides primary-team approvers.",
              detail: teams + " teams configured",
              onClick: function () { store.dispatch({ type: "view/set", payload: "projects" }); }
            }),
            h(ActionCard, {
              title: "HR control panel",
              description: "Allow or pause half-day leave for employees.",
              detail: state.data.hrPolicy && state.data.hrPolicy.allowHalfDayLeave ? "Half day enabled" : "Half day disabled",
              onClick: function () { store.dispatch({ type: "view/set", payload: "hrControl" }); }
            }),
            h(ActionCard, {
              title: "Leave balances",
              description: "Upload Excel sheets for existing employees and keep balances current.",
              detail: "Supports bulk HR uploads",
              onClick: function () { store.dispatch({ type: "view/set", payload: "balances" }); }
            })
          )
        )
      ),
      h("section", { className: "panel" },
        h("div", { className: "panel-header" },
          h("div", null,
            h("h2", { className: "panel-title" }, "For me as an employee"),
            h("div", { className: "panel-subtitle" }, "HR team members still have their own leave and self-onboarding responsibilities here.")
          )
        ),
        h("div", { className: "panel-body" },
          h("div", { className: "action-grid action-grid-compact" },
            h(ActionCard, {
              title: "My leave requests",
              description: "Raise your own leave request. HR requests flow to the head of the organization.",
              onClick: function () { store.dispatch({ type: "view/set", payload: "apply" }); }
            }),
            h(ActionCard, {
              title: "My onboarding details",
              description: "Fill part 2 details like PAN, Aadhaar, experience proof, and personal documents.",
              onClick: function () { store.dispatch({ type: "view/set", payload: "onboarding" }); }
            }),
            h(ActionCard, {
              title: "Employee directory",
              description: "Check current structure, team ownership, and approver mapping.",
              onClick: function () { store.dispatch({ type: "view/set", payload: "directory" }); }
            })
          )
        )
      )
    );
  }

  function ActionCard(props) {
    return h("button", {
      type: "button",
      className: "action-card",
      onClick: props.onClick
    },
      h("strong", null, props.title),
      h("p", null, props.description),
      props.detail ? h("span", null, props.detail) : null
    );
  }

  function HrControlPanel(props) {
    const state = props.state;

    async function savePolicy(event) {
      event.preventDefault();
      try {
        const payload = await api("/api/workspace/hr-policy", {
          method: "PUT",
          body: JSON.stringify({
            allowHalfDayLeave: Boolean(state.hrPolicyForm.allowHalfDayLeave)
          })
        });
        store.dispatch({ type: "message/set", payload: { type: "success", text: payload.message || "HR policy updated." } });
        await loadWorkspace();
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    return h("section", { className: "workspace-stack" },
      h("section", { className: "panel" },
        h("div", { className: "panel-header" },
          h("div", null,
            h("h2", { className: "panel-title" }, "Leave policy"),
            h("div", { className: "panel-subtitle" }, "Control whether employees can request half-day leave.")
          )
        ),
        h("form", { className: "panel-body", onSubmit: savePolicy },
          h("label", { className: "toggle-field policy-toggle" },
            h("input", {
              type: "checkbox",
              checked: Boolean(state.hrPolicyForm.allowHalfDayLeave),
              onChange: function (event) {
                store.dispatch({
                  type: "hrPolicyForm/update",
                  form: "hrPolicyForm",
                  field: "allowHalfDayLeave",
                  value: event.target.checked
                });
              }
            }),
            h("span", null, "Allow employees to take half-day leave")
          ),
          h("div", { style: { marginTop: "16px" } },
            h("button", { className: "primary-button", type: "submit" }, "Save leave policy")
          )
        )
      )
    );
  }

  function LeaveHome(props) {
    const state = props.state;
    const employee = currentEmployee(state);
    const approver = approverForEmployee(employee, state.data);
    const balances = employee && employee.leaveBalances ? employee.leaveBalances : [];
    const myLeaveRequests = employee && state.myLeaves.employeeId === String(employee.id)
      ? state.myLeaves.requests
      : [];

    React.useEffect(function () {
      if (employee) {
        loadMyLeaveRequests(employee.id);
      }
    }, [employee ? employee.id : ""]);

    async function submitLeave(event) {
      event.preventDefault();
      try {
        await api("/api/leave/apply-leave", {
          method: "POST",
          body: JSON.stringify({
            employeeId: Number(state.leaveForm.employeeId),
            leaveTypeId: Number(state.leaveForm.leaveTypeId),
            startDate: state.leaveForm.startDate,
            endDate: state.leaveForm.endDate,
            isHalfDay: Boolean(state.leaveForm.isHalfDay),
            reason: state.leaveForm.reason
          })
        });
        store.dispatch({ type: "leaveForm/reset" });
        store.dispatch({ type: "message/set", payload: { type: "success", text: "Leave request submitted to the default approver." } });
        if (employee) {
          await Promise.all([loadWorkspace(), loadMyLeaveRequests(employee.id)]);
        }
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    async function cancelRequest(request) {
      if (!employee) return;

      const confirmed = window.confirm("Cancel this leave request?");
      if (!confirmed) return;

      try {
        const payload = await cancelLeaveRequest(request.id, employee.id);
        store.dispatch({
          type: "message/set",
          payload: { type: "success", text: payload.message || "Leave request cancelled." }
        });
        await Promise.all([loadWorkspace(), loadMyLeaveRequests(employee.id)]);
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    return h("section", { className: "panel" },
      h("div", { className: "panel-header" },
        h("div", null,
          h("h2", { className: "panel-title" }, "Apply for leave"),
          h("div", { className: "panel-subtitle" }, "Preview the approver before the request is sent.")
        )
      ),
      h("form", { className: "panel-body", onSubmit: submitLeave },
        h("div", { className: "grid-2" },
          h("div", { className: "full-span" },
            h("div", { className: "identity-card" },
              h("div", { className: "meta" }, "Applying as"),
              h("h3", null, employee ? employee.fullName : "Signed-in employee"),
              h("div", { className: "tag-row" },
                employee ? h("span", { className: "tag" }, employee.employeeCode) : null,
                employee ? h("span", { className: "tag" }, roleLabel(employee.role, employee.roleLabel)) : null,
                employee && employee.primaryTeam ? h("span", { className: "tag" }, employee.primaryTeam.name) : null
              )
            )
          ),
          h(Field, { label: "Leave type" },
            h("select", {
              value: state.leaveForm.leaveTypeId,
              onChange: function (event) { store.dispatch({ type: "leaveForm/update", form: "leaveForm", field: "leaveTypeId", value: event.target.value }); }
            }, state.data.leaveTypes.map(function (item) {
              return h("option", { key: item.id, value: item.id }, item.name);
            }))
          ),
          h(Field, { label: "From" },
            h("input", {
              type: "date",
              value: state.leaveForm.startDate,
              onChange: function (event) {
                store.dispatch({ type: "leaveForm/update", form: "leaveForm", field: "startDate", value: event.target.value });
              },
              required: true
            })
          ),
          h(Field, { label: "To" },
            h("input", {
              type: "date",
              value: state.leaveForm.endDate,
              onChange: function (event) { store.dispatch({ type: "leaveForm/update", form: "leaveForm", field: "endDate", value: event.target.value }); },
              required: true
            })
          ),
          state.data.hrPolicy && state.data.hrPolicy.allowHalfDayLeave
            ? h("div", { className: "full-span" },
                h("label", { className: "toggle-field" },
                  h("input", {
                    type: "checkbox",
                    checked: Boolean(state.leaveForm.isHalfDay),
                    onChange: function (event) {
                      store.dispatch({ type: "leaveForm/update", form: "leaveForm", field: "isHalfDay", value: event.target.checked });
                    }
                  }),
                  h("span", null, "Half day for each selected date")
                )
              )
            : null,
          h("div", { className: "full-span" },
            h(Field, { label: "Reason" },
              h("textarea", {
                value: state.leaveForm.reason,
                onChange: function (event) { store.dispatch({ type: "leaveForm/update", form: "leaveForm", field: "reason", value: event.target.value }); },
                placeholder: "A short note for the approver",
                required: true
              })
            )
          )
        ),
        h("div", { className: "section-rule" }),
        h("div", { className: "approver-card" },
          h("div", { className: "meta" }, "Default approver"),
          h("h3", null, approver),
          h("div", { className: "tag-row" },
            employee && employee.primaryTeam ? h("span", { className: "tag" }, employee.primaryTeam.name) : null,
            employee ? h("span", { className: "tag" }, roleLabel(employee.role, employee.roleLabel)) : null
          )
        ),
        h("div", { className: "section-rule" }),
        h("div", { className: "balance-header" },
          h("div", null,
            h("h3", { className: "panel-title small-title" }, "Leave balance snapshot"),
            h("div", { className: "meta" }, "A quick read on used versus remaining balance by category.")
          )
        ),
        balances.length
          ? h("div", { className: "balance-grid" },
              balances.map(function (balance) {
                const summary = balanceSummary(balance);
                return h("article", { className: "balance-card", key: balance.leaveTypeId },
                  h("div", { className: "balance-ring", style: summary.ringStyle },
                    h("div", { className: "balance-ring-center" },
                      h("strong", null, summary.remaining),
                      h("span", null, "left")
                    )
                  ),
                  h("div", { className: "balance-copy" },
                    h("h3", null, balance.leaveTypeName),
                    h("div", { className: "meta" }, summary.used + " used of " + summary.allocated),
                    h("div", { className: "balance-stats" },
                      h("span", null, "Available: " + summary.remaining),
                      h("span", null, "Allocated: " + summary.allocated)
                    )
                  )
                );
              })
            )
          : h("div", { className: "upload-summary" },
              h("div", { className: "meta" }, "No leave balance records are attached to this employee yet.")
        ),
        h("div", { style: { marginTop: "16px" } },
          h("button", { className: "primary-button", type: "submit" }, "Submit leave request")
        ),
        h("div", { className: "section-rule" }),
        h("div", { className: "balance-header" },
          h("div", null,
            h("h3", { className: "panel-title small-title" }, "My leave requests"),
            h("div", { className: "meta" }, "Pending and approved requests can be cancelled from here.")
          )
        ),
        state.myLeaves.loading
          ? h("div", { className: "upload-summary" },
              h("div", { className: "meta" }, "Loading leave requests...")
            )
          : myLeaveRequests.length
            ? h("div", { className: "review-list leave-history-list" },
                myLeaveRequests.map(function (request) {
                  return h("article", { className: "review-card leave-request-card", key: request.id },
                    h("div", { className: "review-card-head" },
                      h("div", null,
                        h("h3", null, request.leaveTypeName),
                        h("div", { className: "meta" }, "Approver: " + (request.approverName || "Not assigned"))
                      ),
                      h("div", { className: "tag-row review-tags" },
                        h("span", { className: "tag " + statusClass(request.status) }, request.status),
                        h("span", { className: "tag" }, formatDays(request.totalDays))
                      )
                    ),
                    h("div", { className: "review-grid" },
                      reviewMeta("From", formatDate(request.fromDate)),
                      reviewMeta("To", formatDate(request.toDate)),
                      reviewMeta("Applied", formatDate(request.appliedOn)),
                      reviewMeta(request.status === "Cancelled" ? "Cancelled" : "Actioned", request.actionedOn ? formatDate(request.actionedOn) : "Not yet")
                    ),
                    h("div", { className: "meta" }, request.reason || "No note"),
                    request.canCancel
                      ? h("div", { className: "review-actions" },
                          h("button", { className: "danger-button", type: "button", onClick: function () { cancelRequest(request); } }, "Cancel request")
                        )
                      : null
                  );
                })
              )
            : h("div", { className: "upload-summary" },
                h("div", { className: "meta" }, "No leave requests submitted yet.")
              )
      )
    );
  }

  function EmployeeOnboarding(props) {
    const state = props.state;
    const employee = currentEmployee(state);
    const experienceLetterRef = React.useRef(null);
    const salarySlipsRef = React.useRef(null);
    const additionalDocumentsRef = React.useRef(null);

    React.useEffect(function () {
      if (state.onboardingForm.employeeId) {
        loadOnboardingProfile(state.onboardingForm.employeeId);
      }
    }, [state.onboardingForm.employeeId]);

    async function submitOnboarding(event) {
      event.preventDefault();

      const formData = new FormData();
      formData.append("employeeId", state.onboardingForm.employeeId);
      formData.append("panNumber", state.onboardingForm.panNumber);
      formData.append("aadhaarNumber", state.onboardingForm.aadhaarNumber);
      formData.append("hasPriorExperience", String(state.onboardingForm.hasPriorExperience));
      formData.append("previousEmployerName", state.onboardingForm.previousEmployerName);
      formData.append("yearsOfExperience", state.onboardingForm.yearsOfExperience || "");
      formData.append("relievingEmailForwarded", String(state.onboardingForm.relievingEmailForwarded));

      const experienceLetter = experienceLetterRef.current && experienceLetterRef.current.files
        ? experienceLetterRef.current.files[0]
        : null;

      if (experienceLetter) {
        formData.append("experienceLetter", experienceLetter);
      }

      const salarySlips = salarySlipsRef.current && salarySlipsRef.current.files
        ? Array.from(salarySlipsRef.current.files)
        : [];

      salarySlips.forEach(function (file) {
        formData.append("salarySlips", file);
      });

      const additionalDocuments = additionalDocumentsRef.current && additionalDocumentsRef.current.files
        ? Array.from(additionalDocumentsRef.current.files)
        : [];

      additionalDocuments.forEach(function (file) {
        formData.append("additionalDocuments", file);
      });

      try {
        const response = await fetch("/api/onboarding", {
          method: "POST",
          body: formData
        });
        const payload = await response.json().catch(function () { return {}; });
        if (!response.ok) {
          throw new Error(payload.message || payload.Message || "Onboarding profile could not be saved.");
        }

        store.dispatch({
          type: "message/set",
          payload: { type: "success", text: payload.message || "Onboarding profile saved." }
        });

        if (experienceLetterRef.current) experienceLetterRef.current.value = "";
        if (salarySlipsRef.current) salarySlipsRef.current.value = "";
        if (additionalDocumentsRef.current) additionalDocumentsRef.current.value = "";

        await loadOnboardingProfile(state.onboardingForm.employeeId);
      } catch (error) {
        store.dispatch({
          type: "message/set",
          payload: { type: "error", text: error.message }
        });
      }
    }

    return h("section", { className: "panel" },
      h("div", { className: "panel-header" },
        h("div", null,
          h("h2", { className: "panel-title" }, "Employee onboarding - part 2"),
          h("div", { className: "panel-subtitle" }, "Capture employee-supplied identity details, prior-experience proofs, and onboarding paperwork in one place.")
        )
      ),
      h("form", { className: "panel-body", onSubmit: submitOnboarding },
        h("div", { className: "grid-2" },
          h("div", { className: "full-span" },
            h("div", { className: "identity-card" },
              h("div", { className: "meta" }, "Profile owner"),
              h("h3", null, employee ? employee.fullName : "Signed-in employee"),
              h("div", { className: "tag-row" },
                employee ? h("span", { className: "tag" }, employee.employeeCode) : null,
                employee ? h("span", { className: "tag" }, roleLabel(employee.role, employee.roleLabel)) : null,
                employee ? h("span", { className: "tag" }, employee.department) : null,
                employee ? h("span", { className: "tag" }, employee.designation) : null,
                employee && employee.jobRole ? h("span", { className: "tag" }, employee.jobRole) : null
              )
            )
          ),
          h(Field, { label: "PAN number" },
            h("input", {
              value: state.onboardingForm.panNumber,
              onChange: function (event) { store.dispatch({ type: "onboardingForm/update", form: "onboardingForm", field: "panNumber", value: event.target.value.toUpperCase() }); },
              placeholder: "ABCDE1234F",
              required: true
            })
          ),
          h(Field, { label: "Aadhaar number" },
            h("input", {
              value: state.onboardingForm.aadhaarNumber,
              onChange: function (event) { store.dispatch({ type: "onboardingForm/update", form: "onboardingForm", field: "aadhaarNumber", value: event.target.value }); },
              placeholder: "123412341234",
              required: true
            })
          ),
          h(Field, { label: "Previous experience" },
            h("label", { className: "toggle-field" },
              h("input", {
                type: "checkbox",
                checked: !state.onboardingForm.hasPriorExperience,
                onChange: function (event) {
                  const hasPriorExperience = !event.target.checked;
                  store.dispatch({ type: "onboardingForm/update", form: "onboardingForm", field: "hasPriorExperience", value: hasPriorExperience });
                  if (!hasPriorExperience) {
                    store.dispatch({ type: "onboardingForm/update", form: "onboardingForm", field: "previousEmployerName", value: "" });
                    store.dispatch({ type: "onboardingForm/update", form: "onboardingForm", field: "yearsOfExperience", value: "" });
                    store.dispatch({ type: "onboardingForm/update", form: "onboardingForm", field: "relievingEmailForwarded", value: false });
                  }
                }
              }),
              h("span", null, "No previous experience")
            )
          )
        ),
        state.onboardingForm.hasPriorExperience
          ? h("div", { className: "onboarding-stack" },
              h("div", { className: "section-rule" }),
              h("div", { className: "grid-2" },
                h(Field, { label: "Previous employer" },
                  h("input", {
                    value: state.onboardingForm.previousEmployerName,
                    onChange: function (event) { store.dispatch({ type: "onboardingForm/update", form: "onboardingForm", field: "previousEmployerName", value: event.target.value }); },
                    placeholder: "Previous company name"
                  })
                ),
                h(Field, { label: "Years of experience" },
                  h("input", {
                    type: "number",
                    min: "0",
                    step: "0.1",
                    value: state.onboardingForm.yearsOfExperience,
                    onChange: function (event) { store.dispatch({ type: "onboardingForm/update", form: "onboardingForm", field: "yearsOfExperience", value: event.target.value }); },
                    placeholder: "3.5"
                  })
                ),
                h("div", { className: "full-span" },
                  h("label", { className: "toggle-field" },
                    h("input", {
                      type: "checkbox",
                      checked: state.onboardingForm.relievingEmailForwarded,
                      onChange: function (event) { store.dispatch({ type: "onboardingForm/update", form: "onboardingForm", field: "relievingEmailForwarded", value: event.target.checked }); }
                    }),
                    h("span", null, "Relieving email has been forwarded to HR")
                  )
                ),
                h(Field, { label: "Experience letter" },
                  h("input", {
                    ref: experienceLetterRef,
                    type: "file",
                    accept: ".pdf,.png,.jpg,.jpeg,.doc,.docx"
                  })
                ),
                h(Field, { label: "Previous salary slips" },
                  h("input", {
                    ref: salarySlipsRef,
                    type: "file",
                    multiple: true,
                    accept: ".pdf,.png,.jpg,.jpeg"
                  })
                )
              )
            )
          : h("div", { className: "upload-summary", style: { marginTop: "18px" } },
              h("div", { className: "meta" }, "This employee is marked as a fresher, so prior-experience documents are not required.")
            ),
        h("div", { className: "section-rule" }),
        h("div", { className: "grid-2" },
          h(Field, { label: "Additional documents" },
            h("input", {
              ref: additionalDocumentsRef,
              type: "file",
              multiple: true,
              accept: ".pdf,.png,.jpg,.jpeg,.doc,.docx"
            })
          ),
          h("div", { className: "upload-summary" },
            h("div", { className: "meta" }, "Document vault"),
            state.onboardingForm.documents.length
              ? h("div", { className: "document-list" },
                  state.onboardingForm.documents.map(function (document) {
                    return h("a", {
                      key: document.id,
                      className: "document-item",
                      href: "/api/onboarding/documents/" + document.id,
                      target: "_blank",
                      rel: "noreferrer"
                    },
                      h("strong", null, document.documentType),
                      h("span", null, document.originalFileName)
                    );
                  })
                )
              : h("div", { className: "meta" }, "No onboarding documents uploaded yet.")
          )
        ),
        h("div", { style: { marginTop: "16px" } },
          h("button", { className: "primary-button", type: "submit" }, "Save onboarding profile")
        )
      )
    );
  }

  function HrRegistration(props) {
    const state = props.state;
    const teams = allTeams(state.data.projects);
    const approver = approverForRegistration(state);
    const selectedProjects = Array.from(new Set(
      teams
        .filter(function (team) { return state.employeeForm.teamIds.includes(String(team.id)); })
        .map(function (team) { return team.projectName; })
    ));

    async function submitEmployee(event) {
      event.preventDefault();
      try {
        const payload = await api("/api/workspace/employees", {
          method: "POST",
          body: JSON.stringify({
            employeeCode: state.employeeForm.employeeCode,
            fullName: state.employeeForm.fullName,
            email: state.employeeForm.email,
            department: state.employeeForm.department,
            designation: state.employeeForm.designation,
            jobRole: state.employeeForm.jobRole,
            employmentType: state.employeeForm.employmentType,
            location: state.employeeForm.location,
            salaryStructureDetails: state.employeeForm.salaryStructureDetails,
            joinDate: state.employeeForm.joinDate,
            role: Number((selectedRole(state) || {}).baseRoleId || state.employeeForm.role),
            roleSelection: String(state.employeeForm.role),
            primaryTeamId: Number(state.employeeForm.primaryTeamId),
            teamIds: state.employeeForm.teamIds.map(Number)
          })
        });
        store.dispatch({ type: "employeeForm/reset" });
        store.dispatch({
          type: "message/set",
          payload: {
            type: "success",
            text: (payload.message || "Employee registered.") + " Username: " + payload.loginUsername + " | Temporary password: " + payload.temporaryPassword
          }
        });
        await loadWorkspace();
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    return h("section", { className: "panel" },
      h("div", { className: "panel-header" },
        h("div", null,
          h("h2", { className: "panel-title" }, "New employee onboarding - part 1"),
          h("div", { className: "panel-subtitle" }, "This is the HR-owned onboarding stage for official company details, allocations, and system setup.")
        )
      ),
      h("form", { className: "panel-body", onSubmit: submitEmployee },
        h("div", { className: "grid-2" },
          h(Field, { label: "Empcode" },
            h("input", {
              value: state.employeeForm.employeeCode,
              onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "employeeCode", value: event.target.value }); },
              placeholder: "EMP010",
              required: true
            })
          ),
          h(Field, { label: "Full name" },
            h("input", {
              value: state.employeeForm.fullName,
              onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "fullName", value: event.target.value }); },
              placeholder: "Ananya Mehta",
              required: true
            })
          ),
          h(Field, { label: "Official email" },
            h("input", {
              type: "email",
              value: state.employeeForm.email,
              onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "email", value: event.target.value }); },
              placeholder: "ananya@company.com",
              required: true
            })
          ),
          h(Field, { label: "Department" },
            h("input", {
              value: state.employeeForm.department,
              onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "department", value: event.target.value }); },
              placeholder: "Engineering",
              required: true
            })
          ),
          h(Field, { label: "Designation" },
            h("input", {
              value: state.employeeForm.designation,
              onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "designation", value: event.target.value }); },
              placeholder: "Senior Analyst",
              required: true
            })
          ),
          h(Field, { label: "Role" },
            h("input", {
              value: state.employeeForm.jobRole,
              onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "jobRole", value: event.target.value }); },
              placeholder: ".NET Developer",
              required: true
            })
          ),
          h(Field, { label: "Join date" },
            h("input", {
              type: "date",
              value: state.employeeForm.joinDate,
              onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "joinDate", value: event.target.value }); },
              required: true
            })
          ),
          h(Field, { label: "Employment type" },
            h("select", {
              value: state.employeeForm.employmentType,
              onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "employmentType", value: event.target.value }); }
            }, [
              h("option", { key: "full-time", value: "Full-time" }, "Full-time"),
              h("option", { key: "contract", value: "Contract" }, "Contract"),
              h("option", { key: "intern", value: "Intern" }, "Intern"),
              h("option", { key: "consultant", value: "Consultant" }, "Consultant")
            ])
          ),
          h(Field, { label: "Location" },
            h("input", {
              value: state.employeeForm.location,
              onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "location", value: event.target.value }); },
              placeholder: "Bengaluru",
              required: true
            })
          ),
          h(Field, { label: "System access" },
            h("select", {
              value: state.employeeForm.role,
              onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "role", value: event.target.value }); }
            }, state.data.roles.map(function (role) {
              return h("option", { key: role.id, value: role.id }, cleanRoleLabel(role.label || role.name));
            }))
          ),
          h("div", { className: "full-span" },
            h(Field, { label: "Salary structure details" },
              h("textarea", {
                value: state.employeeForm.salaryStructureDetails,
                onChange: function (event) { store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "salaryStructureDetails", value: event.target.value }); },
                placeholder: "CTC 9.5 LPA | Fixed 8.1 LPA | Variable 1.4 LPA",
                required: true
              })
            )
          ),
          h("div", { className: "full-span" },
            h(Field, { label: "Primary team" },
              h("select", {
                value: state.employeeForm.primaryTeamId,
                onChange: function (event) {
                  const teamId = event.target.value;
                  store.dispatch({ type: "employeeForm/update", form: "employeeForm", field: "primaryTeamId", value: teamId });
                  if (!state.employeeForm.teamIds.includes(teamId)) {
                    store.dispatch({ type: "employeeForm/toggleTeam", teamId: teamId });
                  }
                },
                required: true
              }, teams.map(function (team) {
                return h("option", { key: team.id, value: team.id }, team.name + " - " + team.projectName);
              }))
            )
          )
        ),
        h("div", { className: "section-rule" }),
        h("div", { className: "grid-2" },
          h("div", { className: "upload-summary" },
            h("div", { className: "meta" }, "Projects assignment"),
            selectedProjects.length
              ? h("div", { className: "tag-row" },
                  selectedProjects.map(function (projectName) {
                    return h("span", { className: "tag", key: projectName }, projectName);
                  })
                )
              : h("div", { className: "meta" }, "Project assignment will appear from selected teams.")
          ),
          h("div", { className: "approver-card" },
            h("div", { className: "meta" }, "Default approver"),
            h("h3", null, approver),
            h("div", { className: "meta" }, "After this step, the employee receives part 2 onboarding access.")
          )
        ),
        h("div", { className: "section-rule" }),
        h("div", { className: "team-grid" },
          teams.map(function (team) {
            return h("label", { className: "team-option", key: team.id },
              h("input", {
                type: "checkbox",
                checked: state.employeeForm.teamIds.includes(String(team.id)),
                onChange: function () { store.dispatch({ type: "employeeForm/toggleTeam", teamId: String(team.id) }); }
              }),
              "Attach team",
              h("strong", null, team.name),
              h("span", null, team.projectName + " - Lead: " + team.leadName)
            );
          })
        ),
        h("div", { style: { marginTop: "16px" } },
          h("button", { className: "primary-button", type: "submit" }, "Complete HR onboarding")
        )
      )
    );
  }

  function ProjectBuilder(props) {
    const state = props.state;
    const teams = allTeams(state.data.projects);
    const [selectedProjectId, setSelectedProjectId] = React.useState("");
    const [updatedProjectName, setUpdatedProjectName] = React.useState("");
    const [selectedTeamId, setSelectedTeamId] = React.useState("");
    const [updatedTeamName, setUpdatedTeamName] = React.useState("");
    const [updatedTeamProjectId, setUpdatedTeamProjectId] = React.useState("");
    const [updatedTeamLeadId, setUpdatedTeamLeadId] = React.useState("");

    const selectedProject = state.data.projects.find(function (project) {
      return String(project.id) === String(selectedProjectId);
    });

    const selectedTeam = teams.find(function (team) {
      return String(team.id) === String(selectedTeamId);
    });

    React.useEffect(function () {
      const firstProject = state.data.projects[0];
      if (!selectedProjectId && firstProject) {
        setSelectedProjectId(String(firstProject.id));
        setUpdatedProjectName(firstProject.name);
      }

      const firstTeam = teams[0];
      if (!selectedTeamId && firstTeam) {
        setSelectedTeamId(String(firstTeam.id));
        setUpdatedTeamName(firstTeam.name);
        setUpdatedTeamProjectId(String(firstTeam.projectId));
        setUpdatedTeamLeadId(String(firstTeam.leadId));
      }
    }, [state.data.projects]);

    function selectProject(projectId) {
      const project = state.data.projects.find(function (item) {
        return String(item.id) === String(projectId);
      });
      setSelectedProjectId(projectId);
      setUpdatedProjectName(project ? project.name : "");
    }

    function selectTeam(teamId) {
      const team = teams.find(function (item) {
        return String(item.id) === String(teamId);
      });
      setSelectedTeamId(teamId);
      setUpdatedTeamName(team ? team.name : "");
      setUpdatedTeamProjectId(team ? String(team.projectId) : "");
      setUpdatedTeamLeadId(team ? String(team.leadId) : "");
    }

    async function createProject(event) {
      event.preventDefault();
      try {
        await api("/api/workspace/projects", {
          method: "POST",
          body: JSON.stringify({ name: state.projectForm.name })
        });
        store.dispatch({ type: "projectForm/reset" });
        store.dispatch({ type: "message/set", payload: { type: "success", text: "Project created." } });
        await loadWorkspace();
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    async function createTeam(event) {
      event.preventDefault();
      try {
        await api("/api/workspace/teams", {
          method: "POST",
          body: JSON.stringify({
            name: state.teamForm.name,
            projectId: Number(state.teamForm.projectId),
            leadId: Number(state.teamForm.leadId)
          })
        });
        store.dispatch({ type: "teamForm/reset" });
        store.dispatch({ type: "message/set", payload: { type: "success", text: "Team created and assigned to the project." } });
        await loadWorkspace();
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    async function updateProject(event) {
      event.preventDefault();
      if (!selectedProject) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: "Choose a project to update." } });
        return;
      }

      try {
        await api("/api/workspace/projects/" + selectedProject.id, {
          method: "PUT",
          body: JSON.stringify({ name: updatedProjectName })
        });
        store.dispatch({ type: "message/set", payload: { type: "success", text: "Project updated. Employee assignments now show the latest project name." } });
        await loadWorkspace();
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    async function updateTeam(event) {
      event.preventDefault();
      if (!selectedTeam) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: "Choose a team to update." } });
        return;
      }

      try {
        await api("/api/workspace/teams/" + selectedTeam.id, {
          method: "PUT",
          body: JSON.stringify({
            name: updatedTeamName,
            projectId: Number(updatedTeamProjectId),
            leadId: Number(updatedTeamLeadId)
          })
        });
        store.dispatch({ type: "message/set", payload: { type: "success", text: "Team updated. Tagged employees now show the latest team, project, and lead." } });
        await loadWorkspace();
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    return h("section", { className: "panel" },
      h("div", { className: "panel-header" },
        h("div", null,
          h("h2", { className: "panel-title" }, "Projects and teams"),
          h("div", { className: "panel-subtitle" }, "Keep the project structure ready before HR attaches teams to employees.")
        )
      ),
      h("div", { className: "panel-body" },
        h("div", { className: "grid-2" },
          h("form", { onSubmit: createProject },
            h(Field, { label: "New project" },
              h("input", {
                value: state.projectForm.name,
                onChange: function (event) { store.dispatch({ type: "projectForm/update", form: "projectForm", field: "name", value: event.target.value }); },
                placeholder: "Client Portal Modernization",
                required: true
              })
            ),
            h("div", { style: { marginTop: "12px" } },
              h("button", { className: "quiet-button", type: "submit" }, "Add project")
            )
          ),
          h("form", { onSubmit: createTeam },
            h("div", { className: "grid-2" },
              h(Field, { label: "Team name" },
                h("input", {
                  value: state.teamForm.name,
                  onChange: function (event) { store.dispatch({ type: "teamForm/update", form: "teamForm", field: "name", value: event.target.value }); },
                  placeholder: "Experience Team",
                  required: true
                })
              ),
              h(Field, { label: "Project" },
                h("select", {
                  value: state.teamForm.projectId,
                  onChange: function (event) { store.dispatch({ type: "teamForm/update", form: "teamForm", field: "projectId", value: event.target.value }); }
                }, state.data.projects.map(function (project) {
                  return h("option", { key: project.id, value: project.id }, project.name);
                }))
              ),
              h("div", { className: "full-span" },
                h(Field, { label: "Team lead" },
                  h("select", {
                    value: state.teamForm.leadId,
                    onChange: function (event) { store.dispatch({ type: "teamForm/update", form: "teamForm", field: "leadId", value: event.target.value }); }
                  }, state.data.employees.map(function (employee) {
                    return h("option", { key: employee.id, value: employee.id }, employee.fullName + " - " + roleLabel(employee.role, employee.roleLabel));
                  }))
                )
              )
            ),
            h("div", { style: { marginTop: "12px" } },
              h("button", { className: "quiet-button", type: "submit" }, "Add team")
            )
          )
        ),
        h("div", { className: "section-rule" }),
        h("div", { className: "project-stack" },
          h("form", { className: "project-update-form", onSubmit: updateProject },
            h("div", { className: "update-form-title" },
              h("strong", null, "Update project"),
              h("span", null, "Rename a project everywhere it is shown.")
            ),
            h("div", { className: "update-side update-side-current" },
              h("div", { className: "update-side-title" }, "Current"),
              h(Field, { label: "Select project" },
                h("select", {
                  value: selectedProjectId,
                  onChange: function (event) { selectProject(event.target.value); },
                  disabled: !state.data.projects.length
                }, state.data.projects.map(function (project) {
                  return h("option", { key: project.id, value: project.id }, project.name);
                }))
              ),
              h("div", { className: "current-value" },
                h("span", null, "Project name"),
                h("strong", null, selectedProject ? selectedProject.name : "No project selected")
              )
            ),
            h("div", { className: "update-side" },
              h("div", { className: "update-side-title" }, "New"),
              h(Field, { label: "Updated project name" },
                h("input", {
                  value: updatedProjectName,
                  onChange: function (event) { setUpdatedProjectName(event.target.value); },
                  placeholder: selectedProject ? selectedProject.name : "Choose a project first",
                  required: true,
                  disabled: !selectedProject
                })
              )
            ),
            h("div", { className: "update-action" },
              h("button", { className: "quiet-button", type: "submit", disabled: !selectedProject }, "Save project")
            )
          ),
          h("form", { className: "project-update-form", onSubmit: updateTeam },
            h("div", { className: "update-form-title" },
              h("strong", null, "Update team"),
              h("span", null, "Rename a team, move it to another project, or change its lead.")
            ),
            h("div", { className: "update-side update-side-current" },
              h("div", { className: "update-side-title" }, "Current"),
              h(Field, { label: "Select team" },
                h("select", {
                  value: selectedTeamId,
                  onChange: function (event) { selectTeam(event.target.value); },
                  disabled: !teams.length
                }, teams.map(function (team) {
                  return h("option", { key: team.id, value: team.id }, team.name + " - " + team.projectName);
                }))
              ),
              selectedTeam
                ? h("div", { className: "current-value-list" },
                    h("div", { className: "current-value" },
                      h("span", null, "Team"),
                      h("strong", null, selectedTeam.name)
                    ),
                    h("div", { className: "current-value" },
                      h("span", null, "Project"),
                      h("strong", null, selectedTeam.projectName)
                    ),
                    h("div", { className: "current-value" },
                      h("span", null, "Lead"),
                      h("strong", null, selectedTeam.leadName)
                    )
                  )
                : h("div", { className: "meta" }, "No team selected")
            ),
            h("div", { className: "update-side" },
              h("div", { className: "update-side-title" }, "New"),
              h("div", { className: "grid-2" },
                h(Field, { label: "Updated team name" },
                  h("input", {
                    value: updatedTeamName,
                    onChange: function (event) { setUpdatedTeamName(event.target.value); },
                    placeholder: selectedTeam ? selectedTeam.name : "Choose a team first",
                    required: true,
                    disabled: !selectedTeam
                  })
                ),
                h(Field, { label: "Project" },
                  h("select", {
                    value: updatedTeamProjectId,
                    onChange: function (event) { setUpdatedTeamProjectId(event.target.value); },
                    disabled: !selectedTeam
                  }, state.data.projects.map(function (project) {
                    return h("option", { key: project.id, value: project.id }, project.name);
                  }))
                ),
                h("div", { className: "full-span" },
                  h(Field, { label: "Team lead" },
                    h("select", {
                      value: updatedTeamLeadId,
                      onChange: function (event) { setUpdatedTeamLeadId(event.target.value); },
                      disabled: !selectedTeam
                    }, state.data.employees.map(function (employee) {
                      return h("option", { key: employee.id, value: employee.id }, employee.fullName + " - " + roleLabel(employee.role, employee.roleLabel));
                    }))
                  )
                )
              )
            ),
            h("div", { className: "update-action" },
              h("button", { className: "quiet-button", type: "submit", disabled: !selectedTeam }, "Save team")
            )
          )
        )
      )
    );
  }

  function HrBulkUploads(props) {
    const state = props.state;

    return h("section", { className: "workspace-stack" },
      h(ExistingEmployeeUpload, { state: state }),
      h(LeaveBalanceUpload, { state: state })
    );
  }

  function ExistingEmployeeUpload(props) {
    const state = props.state;
    const fileInputRef = React.useRef(null);

    async function submitExistingEmployees(event) {
      event.preventDefault();
      const file = fileInputRef.current && fileInputRef.current.files ? fileInputRef.current.files[0] : null;
      if (!file) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: "Choose an existing employees Excel file before uploading." } });
        return;
      }

      const formData = new FormData();
      formData.append("file", file);

      try {
        const response = await fetch("/api/Excel/upload-existing-employees", {
          method: "POST",
          body: formData
        });
        const payload = await response.json().catch(function () { return {}; });
        if (!response.ok) {
          throw new Error(payload.message || payload.Message || "Existing employee upload failed.");
        }
        store.dispatch({ type: "existingEmployeeUpload/result", payload: payload });
        store.dispatch({ type: "message/set", payload: { type: "success", text: payload.message || "Existing employees uploaded." } });
        await loadWorkspace();
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    return h("section", { className: "panel" },
      h("div", { className: "panel-header" },
        h("div", null,
          h("h2", { className: "panel-title" }, "Upload existing employees"),
          h("div", { className: "panel-subtitle" }, "Bulk-add employees whose onboarding was already completed before the portal launch.")
        )
      ),
      h("form", { className: "panel-body", onSubmit: submitExistingEmployees },
        h("div", { className: "grid-2" },
          h(Field, { label: "Employee Excel file" },
            h("input", {
              ref: fileInputRef,
              type: "file",
              accept: ".xlsx,.xls",
              onChange: function (event) {
                const file = event.target.files && event.target.files[0] ? event.target.files[0].name : "";
                store.dispatch({ type: "existingEmployeeUpload/setFile", payload: file });
              }
            })
          ),
          h("div", { className: "upload-summary" },
            h("div", { className: "meta" }, "Selected file"),
            h("h3", null, state.existingEmployeeUpload.fileName || "No file chosen")
          )
        ),
        h("div", { className: "download-row" },
          h("a", { className: "quiet-button link-button", href: "/api/Excel/existing-employees-template" }, "Download existing employees template")
        ),
        h("div", { style: { marginTop: "16px" } },
          h("button", { className: "primary-button", type: "submit" }, "Upload existing employees")
        ),
        h(UploadResult, { result: state.existingEmployeeUpload.result })
      )
    );
  }

  function LeaveBalanceUpload(props) {
    const state = props.state;
    const fileInputRef = React.useRef(null);

    async function submitExcel(event) {
      event.preventDefault();
      const file = fileInputRef.current && fileInputRef.current.files ? fileInputRef.current.files[0] : null;
      if (!file) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: "Choose an Excel file before uploading." } });
        return;
      }

      const formData = new FormData();
      formData.append("file", file);

      try {
        const response = await fetch("/api/Excel/upload-leave-balances", {
          method: "POST",
          body: formData
        });
        const payload = await response.json().catch(function () { return {}; });
        if (!response.ok) {
          throw new Error(payload.message || payload.Message || "Upload failed.");
        }
        store.dispatch({ type: "excelUpload/result", payload: payload });
        store.dispatch({ type: "message/set", payload: { type: "success", text: payload.message || "Leave balances uploaded." } });
        await loadWorkspace();
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    return h("section", { className: "panel" },
      h("div", { className: "panel-header" },
        h("div", null,
          h("h2", { className: "panel-title" }, "Upload leave balances"),
          h("div", { className: "panel-subtitle" }, "Keep existing employees current by importing leave allocations from Excel.")
        )
      ),
      h("form", { className: "panel-body", onSubmit: submitExcel },
        h("div", { className: "grid-2" },
          h(Field, { label: "Excel file" },
            h("input", {
              ref: fileInputRef,
              type: "file",
              accept: ".xlsx,.xls",
              onChange: function (event) {
                const file = event.target.files && event.target.files[0] ? event.target.files[0].name : "";
                store.dispatch({ type: "excelUpload/setFile", payload: file });
              }
            })
          ),
          h("div", { className: "upload-summary" },
            h("div", { className: "meta" }, "Selected file"),
            h("h3", null, state.excelUpload.fileName || "No file chosen")
          )
        ),
        h("div", { className: "download-row" },
          h("a", { className: "quiet-button link-button", href: "/api/Excel/leave-balance-template" }, "Download sample Excel template")
        ),
        h("div", { style: { marginTop: "16px" } },
          h("button", { className: "primary-button", type: "submit" }, "Upload leave balance sheet")
        ),
        h(UploadResult, { result: state.excelUpload.result })
      )
    );
  }

  function UploadResult(props) {
    const result = props.result;
    if (!result) {
      return null;
    }

    return h("div", { className: "upload-result" },
      h("div", { className: "tag-row" },
        h("span", { className: "tag" }, "Processed: " + result.recordsProcessed),
        result.recordsSkipped !== undefined
          ? h("span", { className: "tag" }, "Skipped: " + result.recordsSkipped)
          : null,
        h("span", { className: "tag" }, "Failed: " + result.recordsFailed)
      ),
      result.errors && result.errors.length
        ? h("div", { className: "upload-errors" },
            result.errors.map(function (error, index) {
              return h("div", { key: index, className: "meta" }, error);
            })
          )
        : null
    );
  }

  function ReviewerInbox(props) {
    const state = props.state;
    const reviewer = currentEmployee(state);

    React.useEffect(function () {
      if (state.reviewer.reviewerId) {
        loadReviewerRequests(state.reviewer.reviewerId);
      }
    }, [state.reviewer.reviewerId]);

    async function decide(requestId, action) {
      try {
        await api("/api/leave/reviewer/decision", {
          method: "POST",
          body: JSON.stringify({
            leaveApplicationId: requestId,
            approverId: Number(state.reviewer.reviewerId),
            action: action
          })
        });
        store.dispatch({
          type: "message/set",
          payload: { type: "success", text: action === "approve" ? "Leave request approved." : "Leave request rejected." }
        });
        await Promise.all([loadReviewerRequests(state.reviewer.reviewerId), loadWorkspace()]);
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    return h("section", { className: "panel" },
      h("div", { className: "panel-header" },
        h("div", null,
          h("h2", { className: "panel-title" }, "Reviewer inbox"),
          h("div", { className: "panel-subtitle" }, "See what your team has sent for approval and respond from here.")
        )
      ),
      h("div", { className: "panel-body" },
        h("div", { className: "grid-2" },
          h("div", { className: "identity-card" },
            h("div", { className: "meta" }, "Approver"),
            h("h3", null, reviewer ? reviewer.fullName : "Signed-in approver"),
            h("div", { className: "tag-row" },
              reviewer ? h("span", { className: "tag" }, roleLabel(reviewer.role, reviewer.roleLabel)) : null,
              reviewer && reviewer.primaryTeam ? h("span", { className: "tag" }, reviewer.primaryTeam.name) : null
            )
          ),
          h("div", { className: "upload-summary" },
            h("div", { className: "meta" }, "Pending requests"),
            h("h3", null, state.reviewer.loading ? "Loading..." : String(state.reviewer.requests.length)),
            h("div", { className: "meta" }, state.reviewer.reviewerName || "No approver profile loaded")
          )
        ),
        h("div", { className: "section-rule" }),
        state.reviewer.requests.length
          ? h("div", { className: "review-list" },
              state.reviewer.requests.map(function (request) {
                return h("article", { className: "review-card", key: request.id },
                  h("div", { className: "review-card-head" },
                    h("div", null,
                      h("h3", null, request.employeeName),
                      h("div", { className: "meta" }, request.employeeCode + " - " + request.employeeRole)
                    ),
                    h("div", { className: "tag-row review-tags" },
                      h("span", { className: "tag" }, request.leaveTypeName),
                      request.primaryTeamName ? h("span", { className: "tag" }, request.primaryTeamName) : null,
                      h("span", { className: "tag" }, formatDays(request.totalDays))
                    )
                  ),
                  h("div", { className: "review-grid" },
                    reviewMeta("From", formatDate(request.fromDate)),
                    reviewMeta("To", formatDate(request.toDate)),
                    reviewMeta("Applied", formatDate(request.appliedOn)),
                    reviewMeta("Reason", request.reason || "No note")
                  ),
                  h("div", { className: "review-actions" },
                    h("button", { className: "primary-button", type: "button", onClick: function () { decide(request.id, "approve"); } }, "Approve"),
                    h("button", { className: "danger-button", type: "button", onClick: function () { decide(request.id, "reject"); } }, "Reject")
                  )
                );
              })
            )
          : h("div", { className: "upload-summary" },
              h("div", { className: "meta" }, state.reviewer.loading ? "Loading pending requests..." : "No pending leave requests are assigned to this reviewer right now.")
            ),
        h("div", { className: "section-rule" }),
        h("div", { className: "balance-header" },
          h("div", null,
            h("h3", { className: "panel-title small-title" }, "Recent decisions"),
            h("div", { className: "meta" }, "Processed requests move here after action.")
          )
        ),
        state.reviewer.recentDecisions.length
          ? h("div", { className: "review-list" },
              state.reviewer.recentDecisions.map(function (request) {
                return h("article", { className: "review-card decision-card", key: request.id },
                  h("div", { className: "review-card-head" },
                    h("div", null,
                      h("h3", null, request.employeeName),
                      h("div", { className: "meta" }, request.employeeCode + " - " + request.employeeRole)
                    ),
                    h("div", { className: "tag-row review-tags" },
                      h("span", { className: "tag " + statusClass(request.status) }, request.status),
                      h("span", { className: "tag" }, request.leaveTypeName),
                      h("span", { className: "tag" }, formatDays(request.totalDays))
                    )
                  ),
                  h("div", { className: "review-grid" },
                    reviewMeta("From", formatDate(request.fromDate)),
                    reviewMeta("To", formatDate(request.toDate)),
                    reviewMeta("Applied", formatDate(request.appliedOn)),
                    reviewMeta(request.status === "Cancelled" ? "Cancelled" : "Actioned", formatDate(request.actionedOn || request.approvedOn || request.rejectedOn))
                  ),
                  h("div", { className: "meta" }, request.reason || "No note")
                );
              })
            )
          : h("div", { className: "upload-summary" },
              h("div", { className: "meta" }, state.reviewer.loading ? "Loading recent decisions..." : "No processed requests for this reviewer yet.")
            )
      )
    );
  }

  function reviewMeta(label, value) {
    return h("div", { className: "review-meta" },
      h("span", null, label),
      h("strong", null, value)
    );
  }

  function Directory(props) {
    const state = props.state;
    const [editingEmployeeId, setEditingEmployeeId] = React.useState(null);
    const rows = React.useMemo(function () {
      return hierarchyRows(state);
    }, [state.data.employees, state.data.projects]);
    const editingEmployee = state.data.employees.find(function (employee) {
      return String(employee.id) === String(editingEmployeeId);
    });
    const canEditDirectory = state.currentUser && (isHrRole(state.currentUser.role) || state.currentUser.role === "OrganizationHead");

    return h("section", { className: "panel" },
      h("div", { className: "panel-header" },
        h("div", null,
          h("h2", { className: "panel-title" }, "Organization hierarchy"),
          h("div", { className: "panel-subtitle" }, "Search across the hierarchy, filter by column, and scan reporting ownership in one grid.")
        )
      ),
      h("div", { className: "panel-body" },
        h(TabulatorHierarchy, {
          rows: rows,
          canEdit: canEditDirectory,
          onEdit: function (employeeId) { setEditingEmployeeId(employeeId); }
        }),
        h("div", { className: "section-rule" }),
        h("div", { className: "hierarchy-notes" },
          h("div", { className: "upload-summary" },
            h("div", { className: "meta" }, "Salary structure"),
            h("div", { className: "meta" }, "Visible in the onboarding and employee records flow, but kept out of the main hierarchy grid for readability.")
          )
        ),
        editingEmployee
          ? h(EmployeeEditModal, {
              employee: editingEmployee,
              state: state,
              onClose: function () { setEditingEmployeeId(null); }
            })
          : null
      )
    );
  }

  function EmployeeEditModal(props) {
    const employee = props.employee;
    const state = props.state;
    const teams = allTeams(state.data.projects);
    const [form, setForm] = React.useState(function () {
      return employeeEditForm(employee, teams);
    });
    const selectedProjects = Array.from(new Set(
      teams
        .filter(function (team) { return form.teamIds.includes(String(team.id)); })
        .map(function (team) { return team.projectName; })
    ));

    React.useEffect(function () {
      setForm(employeeEditForm(employee, teams));
    }, [employee.id, state.data.projects]);

    function update(field, value) {
      setForm(function (current) {
        return { ...current, [field]: value };
      });
    }

    function toggleTeam(teamId) {
      setForm(function (current) {
        const exists = current.teamIds.includes(teamId);
        const teamIds = exists
          ? current.teamIds.filter(function (id) { return id !== teamId; })
          : current.teamIds.concat(teamId);
        return { ...current, teamIds: teamIds };
      });
    }

    async function submitEdit(event) {
      event.preventDefault();
      try {
        const payload = await api("/api/workspace/employees/" + employee.id, {
          method: "PUT",
          body: JSON.stringify({
            employeeCode: form.employeeCode,
            fullName: form.fullName,
            email: form.email,
            department: form.department,
            designation: form.designation,
            jobRole: form.jobRole,
            employmentType: form.employmentType,
            location: form.location,
            salaryStructureDetails: form.salaryStructureDetails,
            joinDate: form.joinDate,
            role: Number((selectedRoleForValue(state.data.roles, form.role) || {}).baseRoleId || form.role),
            roleSelection: String(form.role),
            primaryTeamId: Number(form.primaryTeamId),
            teamIds: form.teamIds.map(Number)
          })
        });
        store.dispatch({
          type: "message/set",
          payload: { type: "success", text: payload.message || "Employee updated." }
        });
        props.onClose();
        await loadWorkspace();
      } catch (error) {
        store.dispatch({ type: "message/set", payload: { type: "error", text: error.message } });
      }
    }

    return h("div", { className: "modal-backdrop", role: "presentation" },
      h("section", { className: "modal-panel", role: "dialog", "aria-modal": "true", "aria-labelledby": "employee-edit-title" },
        h("div", { className: "modal-header" },
          h("div", null,
            h("h2", { id: "employee-edit-title", className: "panel-title" }, "Edit employee"),
            h("div", { className: "panel-subtitle" }, employee.fullName + " - " + employee.employeeCode)
          ),
          h("button", { className: "quiet-button", type: "button", onClick: props.onClose }, "Close")
        ),
        h("form", { className: "modal-body", onSubmit: submitEdit },
          h("div", { className: "grid-2" },
            h(Field, { label: "Empcode" },
              h("input", {
                value: form.employeeCode,
                onChange: function (event) { update("employeeCode", event.target.value); },
                required: true
              })
            ),
            h(Field, { label: "Full name" },
              h("input", {
                value: form.fullName,
                onChange: function (event) { update("fullName", event.target.value); },
                required: true
              })
            ),
            h(Field, { label: "Official email" },
              h("input", {
                type: "email",
                value: form.email,
                onChange: function (event) { update("email", event.target.value); },
                required: true
              })
            ),
            h(Field, { label: "Department" },
              h("input", {
                value: form.department,
                onChange: function (event) { update("department", event.target.value); },
                required: true
              })
            ),
            h(Field, { label: "Designation" },
              h("input", {
                value: form.designation,
                onChange: function (event) { update("designation", event.target.value); },
                required: true
              })
            ),
            h(Field, { label: "Role" },
              h("input", {
                value: form.jobRole,
                onChange: function (event) { update("jobRole", event.target.value); },
                required: true
              })
            ),
            h(Field, { label: "Join date" },
              h("input", {
                type: "date",
                value: form.joinDate,
                onChange: function (event) { update("joinDate", event.target.value); },
                required: true
              })
            ),
            h(Field, { label: "Employment type" },
              h("select", {
                value: form.employmentType,
                onChange: function (event) { update("employmentType", event.target.value); }
              }, [
                h("option", { key: "full-time", value: "Full-time" }, "Full-time"),
                h("option", { key: "contract", value: "Contract" }, "Contract"),
                h("option", { key: "intern", value: "Intern" }, "Intern"),
                h("option", { key: "consultant", value: "Consultant" }, "Consultant")
              ])
            ),
            h(Field, { label: "Location" },
              h("input", {
                value: form.location,
                onChange: function (event) { update("location", event.target.value); },
                required: true
              })
            ),
            h(Field, { label: "System access" },
              h("select", {
                value: form.role,
                onChange: function (event) { update("role", event.target.value); }
              }, state.data.roles.map(function (role) {
                return h("option", { key: role.id, value: role.id }, cleanRoleLabel(role.label || role.name));
              }))
            ),
            h("div", { className: "full-span" },
              h(Field, { label: "Salary structure details" },
                h("textarea", {
                  value: form.salaryStructureDetails,
                  onChange: function (event) { update("salaryStructureDetails", event.target.value); },
                  required: true
                })
              )
            ),
            h("div", { className: "full-span" },
              h(Field, { label: "Primary team" },
                h("select", {
                  value: form.primaryTeamId,
                  onChange: function (event) {
                    const teamId = event.target.value;
                    setForm(function (current) {
                      return {
                        ...current,
                        primaryTeamId: teamId,
                        teamIds: current.teamIds.includes(teamId) ? current.teamIds : current.teamIds.concat(teamId)
                      };
                    });
                  },
                  required: true
                }, teams.map(function (team) {
                  return h("option", { key: team.id, value: team.id }, team.name + " - " + team.projectName);
                }))
              )
            )
          ),
          h("div", { className: "section-rule" }),
          h("div", { className: "grid-2" },
            h("div", { className: "upload-summary" },
              h("div", { className: "meta" }, "Projects assignment"),
              selectedProjects.length
                ? h("div", { className: "tag-row" },
                    selectedProjects.map(function (projectName) {
                      return h("span", { className: "tag", key: projectName }, projectName);
                    })
                  )
                : h("div", { className: "meta" }, "Project assignment will appear from selected teams.")
            ),
            h("div", { className: "approver-card" },
              h("div", { className: "meta" }, "Default approver"),
              h("h3", null, approverForEditedEmployee(form, state.data)),
              h("div", { className: "meta" }, "This preview updates from the selected role and primary team.")
            )
          ),
          h("div", { className: "section-rule" }),
          h("div", { className: "team-grid" },
            teams.map(function (team) {
              return h("label", { className: "team-option", key: team.id },
                h("input", {
                  type: "checkbox",
                  checked: form.teamIds.includes(String(team.id)),
                  onChange: function () { toggleTeam(String(team.id)); }
                }),
                "Attach team",
                h("strong", null, team.name),
                h("span", null, team.projectName + " - Lead: " + team.leadName)
              );
            })
          ),
          h("div", { className: "modal-actions" },
            h("button", { className: "quiet-button", type: "button", onClick: props.onClose }, "Cancel"),
            h("button", { className: "primary-button", type: "submit" }, "Save changes")
          )
        )
      )
    );
  }

  function employeeEditForm(employee, teams) {
    const primaryTeamId = employee.primaryTeamId || (employee.primaryTeam ? employee.primaryTeam.id : "") || (teams[0] ? teams[0].id : "");
    const teamIds = employee.teams && employee.teams.length
      ? employee.teams.map(function (team) { return String(team.id); })
      : [];
    const primaryTeamText = primaryTeamId ? String(primaryTeamId) : "";

    return {
      employeeCode: employee.employeeCode || "",
      fullName: employee.fullName || "",
      email: employee.email || "",
      department: employee.department || "",
      designation: employee.designation || "",
      jobRole: employee.jobRole || "",
      employmentType: employee.employmentType || "Full-time",
      location: employee.location || "",
      salaryStructureDetails: employee.salaryStructureDetails || "",
      joinDate: dateInputValue(employee.joinDate),
      role: employee.roleSelection || employee.roleId || 1,
      primaryTeamId: primaryTeamText,
      teamIds: primaryTeamText && !teamIds.includes(primaryTeamText) ? teamIds.concat(primaryTeamText) : teamIds
    };
  }

  function approverForEditedEmployee(form, data) {
    const role = data.roles.find(function (item) {
      return String(item.id) === String(form.role);
    });
    const employee = {
      role: baseRoleNameForRole(data.roles, role),
      primaryTeam: allTeams(data.projects).find(function (team) {
        return String(team.id) === String(form.primaryTeamId);
      })
    };

    return approverForEmployee(employee, data);
  }

  function TabulatorHierarchy(props) {
    const tableRef = React.useRef(null);
    const hostRef = React.useRef(null);
    const [searchText, setSearchText] = React.useState("");
    const rows = props.rows || [];
    const canEdit = Boolean(props.canEdit);
    const isTableAvailable = typeof window !== "undefined" && typeof window.Tabulator === "function";

    React.useEffect(function () {
      if (!isTableAvailable || !hostRef.current) {
        return;
      }

      if (tableRef.current && typeof tableRef.current.destroy === "function") {
        tableRef.current.destroy();
        tableRef.current = null;
      }

      tableRef.current = new window.Tabulator(hostRef.current, {
        data: rows,
        layout: "fitColumns",
        responsiveLayout: "hide",
        movableColumns: true,
        pagination: true,
        paginationPageSize: 12,
        paginationSizeSelector: [12, 24, 48],
        paginationCounter: "rows",
        initialSort: [
          { column: "roleRank", dir: "desc" },
          { column: "employee", dir: "asc" }
        ],
        columns: [
          canEdit ? {
            title: "",
            field: "actions",
            width: 92,
            headerSort: false,
            frozen: true,
            formatter: function () {
              return "<button class='grid-edit-button' type='button'>Edit</button>";
            },
            cellClick: function (event, cell) {
              event.preventDefault();
              props.onEdit(cell.getRow().getData().id);
            }
          } : false,
          { title: "Employee", field: "employee", minWidth: 200, headerFilter: "input", frozen: true },
          { title: "Employee Code", field: "employeeCode", minWidth: 120, headerFilter: "input" },
          { title: "Official Email", field: "email", minWidth: 220, headerFilter: "input" },
          { title: "Department", field: "department", minWidth: 140, headerFilter: "input" },
          { title: "Designation", field: "designation", minWidth: 170, headerFilter: "input" },
          { title: "Role", field: "jobRole", minWidth: 170, headerFilter: "input" },
          { title: "System Access", field: "role", minWidth: 150, headerFilter: "input" },
          { title: "Employment", field: "employmentType", minWidth: 130, headerFilter: "input" },
          { title: "Location", field: "location", minWidth: 130, headerFilter: "input" },
          { title: "Primary Team", field: "primaryTeam", minWidth: 150, headerFilter: "input" },
          { title: "Projects and Teams", field: "projectsAndTeams", minWidth: 280, headerFilter: "input" },
          { title: "Approver", field: "approver", minWidth: 180, headerFilter: "input" },
          { title: "Joining Date", field: "joiningDate", minWidth: 140, headerFilter: "input" }
        ].filter(Boolean)
      });

      return function () {
        if (tableRef.current && typeof tableRef.current.destroy === "function") {
          tableRef.current.destroy();
          tableRef.current = null;
        }
      };
    }, [isTableAvailable, canEdit]);

    React.useEffect(function () {
      if (!tableRef.current) {
        return;
      }

      tableRef.current.setData(rows);
    }, [rows]);

    React.useEffect(function () {
      if (!tableRef.current) {
        return;
      }

      tableRef.current.clearFilter();

      if (!searchText.trim()) {
        return;
      }

      tableRef.current.setFilter(function (data, params) {
        var term = params.term;
        return [
          data.employee,
          data.employeeCode,
          data.email,
        data.department,
        data.designation,
        data.jobRole,
        data.role,
          data.employmentType,
          data.location,
          data.primaryTeam,
          data.projectsAndTeams,
          data.approver,
          data.joiningDate
        ]
          .join(" ")
          .toLowerCase()
          .includes(term);
      }, { term: searchText.trim().toLowerCase() });
    }, [searchText]);

    if (!isTableAvailable) {
      return h("div", { className: "table-wrap" },
        h("table", { className: "org-table" },
          h("thead", null,
            h("tr", null,
              h("th", null, "Employee"),
              canEdit ? h("th", null, "Action") : null,
              h("th", null, "Department"),
              h("th", null, "Designation"),
              h("th", null, "Role"),
              h("th", null, "System access"),
              h("th", null, "Employment"),
              h("th", null, "Location"),
              h("th", null, "Primary team"),
              h("th", null, "Projects and teams"),
              h("th", null, "Approver"),
              h("th", null, "Joining date")
            )
          ),
          h("tbody", null,
            rows.map(function (row) {
              return h("tr", { key: row.id },
                h("td", null,
                  h("div", { className: "table-primary" }, row.employee),
                  h("div", { className: "meta" }, row.employeeCode),
                  h("div", { className: "meta" }, row.email)
                ),
                canEdit ? h("td", null,
                  h("button", {
                    className: "quiet-button compact-action",
                    type: "button",
                    onClick: function () { props.onEdit(row.id); }
                  }, "Edit")
                ) : null,
                h("td", null, row.department),
                h("td", null, row.designation),
                h("td", null, row.jobRole),
                h("td", null, h("span", { className: "table-pill" }, row.role)),
                h("td", null, row.employmentType),
                h("td", null, row.location),
                h("td", null, row.primaryTeam),
                h("td", { className: "table-wide" }, row.projectsAndTeams),
                h("td", null, row.approver),
                h("td", null, row.joiningDate)
              );
            })
          )
        )
      );
    }

    return h("div", { className: "grid-shell" },
      h("div", { className: "grid-toolbar" },
        h("div", { className: "grid-search" },
          h("input", {
            type: "search",
            value: searchText,
            placeholder: "Search across the full hierarchy",
            onChange: function (event) {
              setSearchText(event.target.value);
            }
          })
        ),
        h("div", { className: "meta" }, "Use the search box for broad matching and the column header filters for precise filtering.")
      ),
      h("div", { ref: hostRef, className: "tabulator-host" })
    );
  }

  function LeadershipOverview(props) {
    const state = props.state;
    const teamCount = allTeams(state.data.projects).length;
    const hrCount = state.data.employees.filter(function (employee) { return isHrRole(employee.role); }).length;
    const managerCount = state.data.employees.filter(function (employee) { return isManagerRole(employee.role); }).length;

    return h("section", { className: "panel" },
      h("div", { className: "panel-header" },
        h("div", null,
          h("h2", { className: "panel-title" }, "Leadership overview"),
          h("div", { className: "panel-subtitle" }, "A compact read on structure, approval ownership, and current scale.")
        )
      ),
      h("div", { className: "panel-body leadership-metrics" },
        signal(state.data.employees.length, "Employees"),
        signal(state.data.projects.length, "Projects"),
        signal(teamCount, "Teams"),
        signal(managerCount, "Managers"),
        signal(hrCount, "HR members")
      ),
      h("div", { className: "panel-body" },
        h("div", { className: "table-wrap" },
          h("table", { className: "org-table compact-table" },
            h("thead", null,
              h("tr", null,
                h("th", null, "Hierarchy band"),
                h("th", null, "Current count"),
                h("th", null, "Who sits here")
              )
            ),
            h("tbody", null,
              h("tr", null,
                h("td", null, "Organization head"),
                h("td", null, state.data.employees.filter(function (employee) { return employee.role === "OrganizationHead"; }).length),
                h("td", null, state.data.employees.filter(function (employee) { return employee.role === "OrganizationHead"; }).map(function (employee) { return employee.fullName; }).join(", ") || "None")
              ),
              h("tr", null,
                h("td", null, "HR hierarchy"),
                h("td", null, hrCount),
                h("td", null, state.data.employees.filter(function (employee) { return isHrRole(employee.role); }).map(function (employee) { return employee.fullName + " (" + roleLabel(employee.role, employee.roleLabel) + ")"; }).join(", ") || "None")
              ),
              h("tr", null,
                h("td", null, "Manager hierarchy"),
                h("td", null, managerCount),
                h("td", null, state.data.employees.filter(function (employee) { return isManagerRole(employee.role); }).map(function (employee) { return employee.fullName + " (" + roleLabel(employee.role, employee.roleLabel) + ")"; }).join(", ") || "None")
              ),
              h("tr", null,
                h("td", null, "Team leads"),
                h("td", null, state.data.employees.filter(function (employee) { return employee.role === "TeamLead"; }).length),
                h("td", null, state.data.employees.filter(function (employee) { return employee.role === "TeamLead"; }).map(function (employee) { return employee.fullName; }).join(", ") || "None")
              )
            )
          )
        )
      )
    );
  }

  ReactDOM.createRoot(document.getElementById("root")).render(h(App));
})();
