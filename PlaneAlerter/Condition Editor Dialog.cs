﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlaneAlerter {
	/// <summary>
	/// Form for editing conditions
	/// </summary>
	public partial class Condition_Editor :Form {
		/// <summary>
		/// Id of condition to update
		/// </summary>
		int conditionToUpdate;

		/// <summary>
		/// Is this updating the condition?
		/// </summary>
		bool isUpdating = false;

		/// <summary>
		/// Initialise form with a condition to update
		/// </summary>
		/// <param name="_conditionToUpdate"></param>
		public Condition_Editor(int _conditionToUpdate) {
			isUpdating = true;
			conditionToUpdate = _conditionToUpdate;
			//Initialise form options
			initialise();
			//Set form element values from condition info
			Core.Condition c = EditorConditionsList.conditions[conditionToUpdate];
			conditionNameTextBox.Text = c.conditionName;
			emailPropertyComboBox.Text = c.emailProperty.ToString();
			recieverEmailTextBox.Text = string.Join(Environment.NewLine, c.recieverEmails.ToArray());
			alertTypeComboBox.Text = c.alertType.ToString();
			foreach (Core.Trigger trigger in c.triggers.Values) {
				triggerDataGridView.Rows.Add();
				DataGridViewRow newRow = triggerDataGridView.Rows[triggerDataGridView.Rows.Count - 2];
				DataGridViewComboBoxCell comboBoxCell = (DataGridViewComboBoxCell)(newRow.Cells[0]);
				foreach (Core.vrsProperty property in Enum.GetValues(typeof(Core.vrsProperty)))
					comboBoxCell.Items.Add(property.ToString());
				newRow.Cells[0].Value = trigger.Property.ToString();
				newRow.Cells[1].Value = trigger.ComparisonType;
				newRow.Cells[2].Value = trigger.Value;
			}
		}
		
		/// <summary>
		/// Initialise form with no existing condition
		/// </summary>
		public Condition_Editor() {
			//Initialise form options
			initialise();
		}
		
		/// <summary>
		/// Initialise form options
		/// </summary>
		public void initialise() {
			//Initialise form elements
			InitializeComponent();

			//Add vrs properties to triggers table
			IEnumerable<DataGridViewRow> rows = triggerDataGridView.Rows.Cast<DataGridViewRow>();
            foreach(DataGridViewRow row in rows) {
				DataGridViewComboBoxCell comboBoxCell = (DataGridViewComboBoxCell)(row.Cells[0]);
				foreach(Core.vrsProperty property in Enum.GetValues(typeof(Core.vrsProperty)))
					comboBoxCell.Items.Add(property.ToString());
			}

			//Add vrs properties to combobox
			foreach(Core.vrsProperty property in Enum.GetValues(typeof(Core.vrsProperty)))
				emailPropertyComboBox.Items.Add(property.ToString());
			//Add alert types to combobox
			foreach(Core.AlertType property in Enum.GetValues(typeof(Core.AlertType)))
				alertTypeComboBox.Items.Add(property.ToString());
		}
		
		/// <summary>
		/// Trigger table cell value changed
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Event Args</param>
		private void triggerDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
			//Check if cell changed is in the value column and value isnt empty
			if (e.ColumnIndex == 2 && triggerDataGridView.Rows.Count != 1 && triggerDataGridView.Rows[e.RowIndex].Cells[0].Value != null && triggerDataGridView.Rows[e.RowIndex].Cells[0].Value.ToString() != "") {
				//Clear value if property is number and value is not a number
				if (triggerDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString() != "" && Core.vrsPropertyData[(Core.vrsProperty)Enum.Parse(typeof(Core.vrsProperty), triggerDataGridView.Rows[e.RowIndex].Cells[0].Value.ToString())][0] == "Number") {
					try {
						Convert.ToInt32(triggerDataGridView.Rows[e.RowIndex].Cells[2].Value);
					}
					catch(Exception) {
						triggerDataGridView.Rows[e.RowIndex].Cells[2].Value = "";
					}
				}
				//Format squawk to 4 digits e.g. 0010
				if (triggerDataGridView.Rows[e.RowIndex].Cells[0].Value.ToString() == "Squawk" && triggerDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString().Length != 4)
					if (triggerDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString().Length == 1)
						triggerDataGridView.Rows[e.RowIndex].Cells[2].Value = "000" + triggerDataGridView.Rows[e.RowIndex].Cells[2].Value;
					else
						if(triggerDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString().Length == 2)
							triggerDataGridView.Rows[e.RowIndex].Cells[2].Value = "00" + triggerDataGridView.Rows[e.RowIndex].Cells[2].Value;
						else
							if(triggerDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString().Length == 3)
								triggerDataGridView.Rows[e.RowIndex].Cells[2].Value = "0" + triggerDataGridView.Rows[e.RowIndex].Cells[2].Value;
							else
								triggerDataGridView.Rows[e.RowIndex].Cells[2].Value = "";
			}
			//Change comparison type combo box based on property selected
			if (e.ColumnIndex == 0 && triggerDataGridView.Rows.Count != 1) {
				//Clear combobox
				DataGridViewComboBoxCell comparisonTypeComboBox = (DataGridViewComboBoxCell)(triggerDataGridView.Rows[e.RowIndex].Cells[1]);
				comparisonTypeComboBox.Items.Clear();
				comparisonTypeComboBox.Value = "";

				//Get comparison types supported by property
				string supportedComparisonTypes = "";
				try {
					supportedComparisonTypes = Core.vrsPropertyData[(Core.vrsProperty)Enum.Parse(typeof(Core.vrsProperty), triggerDataGridView.Rows[e.RowIndex].Cells[0].Value.ToString())][1];
				}
				catch (Exception) {
					return;
				}
				//Add comparison types to combobox from supported comparison types
				if (supportedComparisonTypes.Contains("A"))
					foreach (string comparisonType in Core.comparisonTypes["A"])
						comparisonTypeComboBox.Items.Add(comparisonType);
				if(supportedComparisonTypes.Contains("B"))
					foreach(string comparisonType in Core.comparisonTypes["B"])
						comparisonTypeComboBox.Items.Add(comparisonType);
				if(supportedComparisonTypes.Contains("C")) {
					foreach(string comparisonType in Core.comparisonTypes["C"])
						comparisonTypeComboBox.Items.Add(comparisonType);
					triggerDataGridView.Rows[e.RowIndex].Cells[2].Value = "True";
					triggerDataGridView.Rows[e.RowIndex].Cells[2].ReadOnly = true;
				}
				else
					triggerDataGridView.Rows[e.RowIndex].Cells[2].Value = "";
					triggerDataGridView.Rows[e.RowIndex].Cells[2].ReadOnly = false;
				if(supportedComparisonTypes.Contains("D"))
					foreach(string comparisonType in Core.comparisonTypes["D"])
						comparisonTypeComboBox.Items.Add(comparisonType);
				if(supportedComparisonTypes.Contains("E"))
					foreach(string comparisonType in Core.comparisonTypes["E"])
						comparisonTypeComboBox.Items.Add(comparisonType);
			}
		}
		
		/// <summary>
		/// Trigger table row added
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Event Args</param>
		void TriggerDataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
		{
			//Iterate rows in table
			foreach (DataGridViewRow row in triggerDataGridView.Rows) {
				DataGridViewComboBoxCell comboBoxCell = (DataGridViewComboBoxCell)(row.Cells[0]);
				//Add vrs properties to combobox
				comboBoxCell.Items.Clear();
				foreach (Core.vrsProperty property in Enum.GetValues(typeof(Core.vrsProperty)))
					comboBoxCell.Items.Add(property.ToString());
			}
		}
		
		/// <summary>
		/// Save button click
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Event Args</param>
		void SaveButtonClick(object sender, EventArgs e)
		{
			//Check if values are empty/invalid
			bool cancelSave = false;
			if (conditionNameTextBox.Text == "") {
				conditionNameLabel.ForeColor = Color.Red;
				cancelSave = true;
			}
			else {
				conditionNameLabel.ForeColor = SystemColors.ControlText;
			}
			if(emailPropertyComboBox.Text == "") {
				emailPropertyLabel.ForeColor = Color.Red;
				cancelSave = true;
			}
			else {
				emailPropertyLabel.ForeColor = SystemColors.ControlText;
			}
			if(recieverEmailTextBox.Text == "") {
				emailToSendToLabel.ForeColor = Color.Red;
				cancelSave = true;
			}
			else {
				emailToSendToLabel.ForeColor = SystemColors.ControlText;
			}
			if(alertTypeComboBox.Text == "") {
				alertTypeLabel.ForeColor = Color.Red;
				cancelSave = true;
			}
			else {
				alertTypeLabel.ForeColor = SystemColors.ControlText;
			}
			if(triggerDataGridView.Rows.Count == 1) {
				triggerDataGridView.BackgroundColor = Color.Red;
				cancelSave = true;
			}
			else {
				triggerDataGridView.BackgroundColor = SystemColors.AppWorkspace;
			}
			//Cancel if values are invalid
			if (cancelSave) {
				return;
			}

			//If condition is being updated, remove the old one
			if (isUpdating)
				EditorConditionsList.conditions.Remove(conditionToUpdate);

			//Sort conditions
			List<int> list = EditorConditionsList.conditions.Keys.ToList();
			SortedDictionary<int, Core.Condition> sortedConditions = new SortedDictionary<int, Core.Condition>();
			list.Sort();
			foreach(var key in list)
				sortedConditions.Add(key, EditorConditionsList.conditions[key]);
			EditorConditionsList.conditions = sortedConditions;

			//Create new condition
			Core.Condition newCondition = new Core.Condition();
			newCondition.conditionName = conditionNameTextBox.Text;
			newCondition.emailProperty = (Core.vrsProperty)Enum.Parse(typeof(Core.vrsProperty), emailPropertyComboBox.Text);
			newCondition.recieverEmails = recieverEmailTextBox.Text.Split(new string[] {Environment.NewLine}, StringSplitOptions.None).ToList();
			newCondition.alertType = (Core.AlertType)Enum.Parse(typeof(Core.AlertType), alertTypeComboBox.Text);
			newCondition.ignoreFollowing = ignoreFollowingCheckbox.Checked;
			if (triggerDataGridView.Rows.Count != 0)
				foreach (DataGridViewRow row in triggerDataGridView.Rows)
					if (row.Index != triggerDataGridView.Rows.Count - 1)
						foreach (Core.vrsProperty property in Enum.GetValues(typeof(Core.vrsProperty)))
							if (property.ToString() == row.Cells[0].Value.ToString()) {
								newCondition.triggers.Add(newCondition.triggers.Count, new Core.Trigger(property, row.Cells[2].Value.ToString(), row.Cells[1].Value.ToString()));
								break;
							}
			//Add condition to condition list
			if (isUpdating)
				EditorConditionsList.conditions.Add(conditionToUpdate, newCondition);
			else
				EditorConditionsList.conditions.Add(EditorConditionsList.conditions.Count, newCondition);
			//Close form
			Close();
		}

		/// <summary>
		/// Property info button click
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Event Args</param>
		private void propertyInfoButton_Click(object sender, EventArgs e) {
			//Show property info form
			PropertyInfoForm propertyInfoForm = new PropertyInfoForm();
			propertyInfoForm.Show();
		}
	}
}
